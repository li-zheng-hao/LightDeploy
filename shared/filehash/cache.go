package filehash

import (
	"container/list"
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"sync"
)

type entry struct {
	path string
	info os.FileInfo
	hash string
}

// Cache 按文件身份、大小和纳秒修改时间复用 SHA-256，使用 LRU 限制内存占用。
// 同一个缓存串行读取文件，避免并发发布重复计算并争用磁盘。
type Cache struct {
	mu       sync.Mutex
	capacity int
	entries  map[string]*list.Element
	recent   *list.List
	compute  func(string) (string, error)
}

func NewCache(capacity int) *Cache {
	if capacity < 1 {
		capacity = 1
	}
	return &Cache{
		capacity: capacity,
		entries:  make(map[string]*list.Element),
		recent:   list.New(),
		compute:  hashFile,
	}
}

func (c *Cache) Hash(path string) (string, error) {
	path, err := filepath.Abs(path)
	if err != nil {
		return "", err
	}
	c.mu.Lock()
	defer c.mu.Unlock()

	info, err := os.Stat(path)
	if err != nil {
		c.remove(path)
		return "", err
	}
	if !info.Mode().IsRegular() {
		return "", fmt.Errorf("不是普通文件: %s", path)
	}
	if element, ok := c.entries[path]; ok {
		cached := element.Value.(entry)
		if sameFileVersion(cached.info, info) {
			c.recent.MoveToFront(element)
			return cached.hash, nil
		}
		c.remove(path)
	}

	hash, err := c.compute(path)
	if err != nil {
		return "", err
	}
	currentInfo, err := os.Stat(path)
	if err != nil {
		return "", err
	}
	if !sameFileVersion(info, currentInfo) {
		return "", fmt.Errorf("计算哈希时文件发生变化，请重试发布: %s", path)
	}
	c.entries[path] = c.recent.PushFront(entry{path: path, info: currentInfo, hash: hash})
	if c.recent.Len() > c.capacity {
		c.remove(c.recent.Back().Value.(entry).path)
	}
	return hash, nil
}

func (c *Cache) remove(path string) {
	if element, ok := c.entries[path]; ok {
		c.recent.Remove(element)
		delete(c.entries, path)
	}
}

func sameFileVersion(a, b os.FileInfo) bool {
	return os.SameFile(a, b) && a.Size() == b.Size() && a.ModTime().Equal(b.ModTime())
}

func hashFile(path string) (string, error) {
	file, err := os.Open(path)
	if err != nil {
		return "", err
	}
	defer file.Close()
	hash := sha256.New()
	if _, err := io.Copy(hash, file); err != nil {
		return "", err
	}
	return hex.EncodeToString(hash.Sum(nil)), nil
}
