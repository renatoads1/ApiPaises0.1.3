using System;
using System.IO;
using System.Net;

namespace SpectrumApiIai.Services
{
    public class FtpService 
    {
        public  string Download(string uri, string user, string senha) {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(uri));
                
                var imgNomeArr = uri.Split("/");
                var imgNome = imgNomeArr[(imgNomeArr.Length - 1)];

                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(user,senha);
                request.UseBinary = true;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream rs = response.GetResponseStream())
                    {
                        using (FileStream ws = new FileStream(Directory.GetCurrentDirectory() + @"\Img\"+ imgNome, FileMode.Create))
                        {
                            byte[] buffer = new byte[2048];
                            int bytesRead = rs.Read(buffer, 0, buffer.Length);
                            while (bytesRead > 0)
                            {
                                ws.Write(buffer, 0, bytesRead);
                                bytesRead = rs.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new NotImplementedException("erro");
            }
            return "ok";
        }



        public string Upload(string nomeimg, string user, string senha)
        {
            try
            {
                FileInfo arquivoInfo = new FileInfo(Directory.GetCurrentDirectory() + "\\Img\\"+ nomeimg);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri("ftp://localhost/"+ nomeimg));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(user, senha);
                request.UseBinary = true;
                request.ContentLength = arquivoInfo.Length;
                using (FileStream fs = arquivoInfo.OpenRead())
                {
                    byte[] buffer = new byte[2048];
                    int bytesSent = 0;
                    int bytes = 0;
                    using (Stream stream = request.GetRequestStream())
                    {
                        while (bytesSent < arquivoInfo.Length)
                        {
                            bytes = fs.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, bytes);
                            bytesSent += bytes;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error"+ex;
            }
            //executa programa de converssão de imagem 
            return "ok";
        }
    }
}
