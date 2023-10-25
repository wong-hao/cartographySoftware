using System.IO;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    /// <summary>
    /// 输出一个文本文件
    /// </summary>
    public class TextFileWriter
    {
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="fpath">文件全路径</param>
        /// <param name="content">文本内容</param>
        ///  <param name="autoOver">是否自动覆盖</param>
        public static ResultMessage SaveFile(string fpath,  string content,bool autoOver=false)
        {
            var rm = new ResultMessage();
            if (File.Exists(fpath)&&!autoOver)
            {
                if (DialogResult.No ==
                    MessageBox.Show(string.Format("文件【{0}】已经存在！是否替换？", fpath), "", MessageBoxButtons.YesNo))
                {
                    rm.stat=ResultState.Cancel;
                    return rm;
                }

                File.Delete(fpath);
            }
            try
            {
                var strm = File.OpenWrite(fpath);
                using (var wr = new StreamWriter(strm))
                {
                    wr.Write(content);
                }
                strm.Close();
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                rm.stat=ResultState.Failed;
                rm.msg = ex.Message;
                return rm;
            }
            rm.stat=ResultState.Ok;
            return rm;
        }

       
    }
}
