package filehash

import (
	"os"
	"path/filepath"
	"sync"
	"testing"
	"time"
)

func TestCacheReusesHashAndInvalidatesChangedFiles(t *testing.T) {
	path := filepath.Join(t.TempDir(), "app.dll")
	write := func(content string, modTime time.Time) {
		t.Helper()
		if err := os.WriteFile(path, []byte(content), 0644); err != nil {
			t.Fatal(err)
		}
		if err := os.Chtimes(path, modTime, modTime); err != nil {
			t.Fatal(err)
		}
	}
	modTime := time.Unix(1700000000, 0)
	write("abc", modTime)
	cache := NewCache(10)
	reads := 0
	cache.compute = func(path string) (string, error) {
		reads++
		return hashFile(path)
	}
	for i := 0; i < 3; i++ {
		hash, err := cache.Hash(path)
		if err != nil {
			t.Fatal(err)
		}
		if hash != "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad" {
			t.Fatalf("unexpected SHA-256: %s", hash)
		}
	}
	if reads != 1 {
		t.Fatalf("unchanged file read %d times, want 1", reads)
	}
	write("xyz", modTime.Add(time.Millisecond))
	hash, err := cache.Hash(path)
	if err != nil {
		t.Fatal(err)
	}
	if hash != "3608bca1e44ea6c4d268eb6db02260269892c0b42b86bbf1e77a6fa16c3c9282" || reads != 2 {
		t.Fatalf("subsecond modification did not invalidate cache: hash=%s, reads=%d", hash, reads)
	}
	write("abcdef", modTime.Add(time.Millisecond))
	if _, err := cache.Hash(path); err != nil {
		t.Fatal(err)
	}
	if reads != 3 {
		t.Fatalf("size change did not invalidate cache: reads=%d", reads)
	}
	if err := os.Remove(path); err != nil {
		t.Fatal(err)
	}
	if _, err := cache.Hash(path); err == nil {
		t.Fatal("deleted file returned a cached hash")
	}
}

func TestCacheBoundsEntries(t *testing.T) {
	dir := t.TempDir()
	cache := NewCache(2)
	reads := 0
	cache.compute = func(path string) (string, error) {
		reads++
		return hashFile(path)
	}
	paths := []string{filepath.Join(dir, "a"), filepath.Join(dir, "b"), filepath.Join(dir, "c")}
	for _, path := range paths {
		if err := os.WriteFile(path, []byte("abc"), 0644); err != nil {
			t.Fatal(err)
		}
		if _, err := cache.Hash(path); err != nil {
			t.Fatal(err)
		}
	}
	if _, err := cache.Hash(paths[1]); err != nil {
		t.Fatal(err)
	}
	if reads != 3 {
		t.Fatal("recent file was evicted instead of oldest file")
	}
	if _, err := cache.Hash(paths[0]); err != nil {
		t.Fatal(err)
	}
	if reads != 4 {
		t.Fatalf("oldest entry was not evicted: reads=%d", reads)
	}
}

func TestCacheConcurrentRequestsReadFileOnce(t *testing.T) {
	path := filepath.Join(t.TempDir(), "app.dll")
	if err := os.WriteFile(path, []byte("abc"), 0644); err != nil {
		t.Fatal(err)
	}
	cache := NewCache(10)
	reads := 0
	cache.compute = func(path string) (string, error) {
		reads++
		return hashFile(path)
	}
	var wg sync.WaitGroup
	for i := 0; i < 10; i++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			if _, err := cache.Hash(path); err != nil {
				t.Error(err)
			}
		}()
	}
	wg.Wait()
	if reads != 1 {
		t.Fatalf("concurrent requests read file %d times, want 1", reads)
	}
}

func TestCacheRejectsFileChangedDuringHashing(t *testing.T) {
	path := filepath.Join(t.TempDir(), "app.dll")
	if err := os.WriteFile(path, []byte("abc"), 0644); err != nil {
		t.Fatal(err)
	}
	cache := NewCache(10)
	cache.compute = func(path string) (string, error) {
		hash, err := hashFile(path)
		if err != nil {
			return "", err
		}
		return hash, os.WriteFile(path, []byte("changed"), 0644)
	}
	if _, err := cache.Hash(path); err == nil {
		t.Fatal("file changed during hashing without an error")
	}
	cache.compute = hashFile
	if _, err := cache.Hash(path); err != nil {
		t.Fatalf("hash failure poisoned the cache: %v", err)
	}
}
