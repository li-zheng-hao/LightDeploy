import {isObject} from "@/utils/is";
import copy from "@/directives/copy";

export function deepMerge<T = any>(src: any = {}, target: any = {}): T {
    let key: string;
    for (key in target) {
        src[key] = isObject(src[key]) ? deepMerge(src[key], target[key]) : (src[key] = target[key]);
    }
    return src;
}

/**
 * 深拷贝一个对象
 * @param source
 */
export function deepClone(source:any) {

    // Prevent undefined objects
    if (!source) return source;

    return JSON.parse(JSON.stringify(source));

}

/**
 * 数组去重
 * @param arr
 */
export function distinct(arr) {
    return Array.from(new Set(arr))
}