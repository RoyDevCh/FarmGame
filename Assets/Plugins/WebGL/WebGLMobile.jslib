// Unity WebGL 插件：提供 WebGLTool.IsMobile() 所需的 __Internal 符号。
// 原始 FarmingEngine 资源包不包含这个文件（它在 Assets/Plugins/WebGL 下，位于 FarmingEngine 文件夹之外），
// 缺少它会导致 WebGL 打包失败：
//   error: undefined symbol: IsMobile (referenced by top-level compiled C/C++ code)
mergeInto(LibraryManager.library, {
  IsMobile: function () {
    return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent) ? 1 : 0;
  },
});
