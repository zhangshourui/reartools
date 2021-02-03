using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class SendMail
    {
        public static bool SendEmail(string mailSubject, string mailContent)
        {
            var mailTo = ConfigurationManager.AppSettings["MailUrl"] ?? "zhouxiong@zhongjiuyun.com";
            // 设置发送方的邮件信息,例如使用网易的smtp
            string smtpServer = "smtp.ym.163.com"; //SMTP服务器
            string mailFrom = "monitor@zhongjiuyun.com"; //登录用户名
            string userPassword = "123qwe";//登录密码

            // 邮件服务设置
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            smtpClient.Host = smtpServer; //指定SMTP服务器
            smtpClient.Credentials = new System.Net.NetworkCredential(mailFrom, userPassword);//用户名和密码

            // 发送邮件设置        
            MailMessage mailMessage = new MailMessage(mailFrom, mailTo); // 发送人和收件人
            mailMessage.Subject = mailSubject;//主题
            mailMessage.Body = mailContent;//内容
            mailMessage.BodyEncoding = Encoding.UTF8;//正文编码
            mailMessage.IsBodyHtml = true;//设置为HTML格式
            mailMessage.Priority = MailPriority.Low;//优先级

            try
            {
                smtpClient.Send(mailMessage); // 发送邮件
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
