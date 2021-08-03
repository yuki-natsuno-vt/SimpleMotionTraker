using System.Diagnostics;
using System.IO;

public class FilePathSelecter
{
    // Unityのエディタ上で実行すると作業ディレクトリが変更された、のようなエラーメッセージが出て強制終了する
    // ビルド済みのexeの場合は問題ない.
    public static string getOpenFileName(string args)
    {
        var workDir = System.IO.Directory.GetCurrentDirectory();

        ProcessStartInfo pInfo = new ProcessStartInfo();
        pInfo.FileName = workDir + "/SelectFile.exe";
        pInfo.Arguments = args; //"-l \"Open Files\" \"Config files(*.cfg)\\0 *.cfg\\0All files(*.*)\\0 *.*\\0\\0\" \"cfg\"";
        Process p = Process.Start(pInfo);
        p.WaitForExit();

        var fileName = File.ReadAllText("temp_path");
        return fileName;
    }

    public static string getSaveFileName(string args)
    {
        var workDir = System.IO.Directory.GetCurrentDirectory();

        ProcessStartInfo pInfo = new ProcessStartInfo();
        pInfo.FileName = workDir + "/SelectFile.exe";
        pInfo.Arguments = args; //"-s \"Save Files\" \"Config files(*.cfg)\\0 *.cfg\\0All files(*.*)\\0 *.*\\0\\0\" \"cfg\"";
        Process p = Process.Start(pInfo);
        p.WaitForExit();

        var fileName = File.ReadAllText("temp_path");
        return fileName;
    }
}
