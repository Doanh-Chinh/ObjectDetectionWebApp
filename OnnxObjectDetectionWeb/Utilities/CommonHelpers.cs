using System.IO;

namespace OnnxObjectDetectionWeb.Utilities
{
    public static class CommonHelpers
    {
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);  // FullName = "D:\\uel\\Nam4\\DeepLearning\\Coding\\FinalProject\\OnnxObjectDetectionWeb\\bin\\Debug\\net5.0\\OnnxObjectDetectionWeb.dll"
            string assemblyFolderPath = _dataRoot.Directory.FullName;  //~/net5.0

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);  // relativePath = "ML/OnnxModels/TinyYolo2_model.onnx"
            return fullPath;
        }
    }
}
