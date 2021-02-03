using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RearTools.Controllers
{
    public class ToolsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        #region convert binary numeric to text
        public IActionResult DataFormater()
        {
            return View();
        }
        public class CvrtData
        {
            public string binContent { get; set; }
            public string encoding { get; set; }
        }


        [HttpPost]
        public AjaxResult DoConvertBin2Text([FromBody] CvrtData data)
        {
            var binContent = data.binContent;
            var encoding = data.encoding;
            if (!CheckBinNumbers(binContent, out string err, out int maxChrCount))
            {
                return AjaxResult.Error(err);
            }
            var buffer = new byte[maxChrCount];
            var binIndex = 0;
            for (var i = 0; i < binContent.Length; i++)
            {
                var c = binContent[i];
                if (c == '0' || c == '1')
                {
                    var n = (uint)(c - '0');
                    var b = (uint)buffer[binIndex / 8];
                    var off = binIndex % 8;
                    b |= n << (7 - off);
                    buffer[binIndex / 8] = (byte)b;

                    binIndex++;
                }
            }
            var tarEncoding = System.Text.Encoding.UTF8;
            if (!string.IsNullOrEmpty(encoding))
            {
                try
                {

                    tarEncoding = System.Text.Encoding.GetEncoding(encoding);

                }
                catch (System.Exception ex)
                {
                    return AjaxResult.Error("编码不支持，" + ex.Message);
                }
                finally
                {

                }
            }
            if (tarEncoding == null)
            {
                AjaxResult.Error("编码不支持");
            }

            return AjaxResult.OK(string.Empty, tarEncoding.GetString(buffer));
        }
        private bool CheckBinNumbers(string content, out string errMsg, out int maxChars)
        {
            errMsg = string.Empty;
            maxChars = 0;
            if (string.IsNullOrEmpty(content))
            {
                errMsg = "没有要转换的内容";
                return false;
            }
            var binCharCount = 0;
            for (var i = 0; i < content.Length; i++)
            {
                var c = content[i];
                if (c == '0' || c == '1')
                {
                    binCharCount++;
                }
                else if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                else
                {
                    errMsg = $"非法字符:\"{c}\"。 位置：{i + 1}";
                    return false;
                }
            }
            if (binCharCount == 0 || binCharCount % 8 != 0)
            {
                errMsg = $"二进制字符（0、1）数量必须是8的整数倍，当前是{binCharCount}";
                return false;
            }
            maxChars = binCharCount / 8;
            return true;
        }
        #endregion

        #region Password generator

        public IActionResult PwdGenerator()
        {

            return View();
        }
        private static readonly System.Random R = new System.Random();

        public class PwdGenerateParameters
        {
            public string candidateChars { get; set; }
            public int len { get; set; }
        }
        public AjaxResult GenerateAPwd([FromBody] PwdGenerateParameters pwdGenerateParameters)
        {
            if (pwdGenerateParameters.candidateChars.Length < 8)
            {
                return AjaxResult.Error("候选字符数不足8个");
            }
            if (pwdGenerateParameters.len <= 0)
            {
                return AjaxResult.Error("没有指定密码长度");
            }

            var pwdChars = new char[pwdGenerateParameters.len];
            int lower = 0, upper = 0, spec = 0, num = 0, matchLen = 0;
            int strength = 0;
            for (var i = 0; i < pwdGenerateParameters.len; i++)
            {
                var chrIndex = R.Next(0, pwdGenerateParameters.candidateChars.Length - 1);
                pwdChars[i] = pwdGenerateParameters.candidateChars[chrIndex];
                if (char.IsNumber(pwdChars[i]))
                {
                    num = 1;
                }
                else if (char.IsLetter(pwdChars[i]) && char.IsUpper(pwdChars[i]))
                {
                    upper = 1;
                }
                else if (char.IsLetter(pwdChars[i]) && char.IsLower(pwdChars[i]))
                {
                    lower = 1;
                }
                else //if ()
                {
                    spec = 1;
                }
            }
            if (pwdGenerateParameters.len > 6)
            {
                matchLen = 1;
            }
            strength = lower + upper + num + spec + matchLen;


            return AjaxResult.OK("", new { pwd = new string(pwdChars), strength, maxStrength = 5 });

        }
        #endregion 
    }

}
