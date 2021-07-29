// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Microsoft.AspNetCore.Mvc;
using Calibration.Api.Models;
using SpectrumApiIai.Services;
using System.IO;
using System.Diagnostics;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using MongoDB.Bson.IO;

namespace Warmup.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/calibration")]  // Support path versioning
    [ApiVersion("1.0-rc1")] // Support path versioning
    public class CalibrationController : ControllerBase
    {
        private readonly ICalibrationOrchestrator calibrationOrchestrator;
        public FtpService ftp = new FtpService();
        public CalibrationController(
            ICalibrationOrchestrator calibrationOrchestrator
            )
        {
            this.calibrationOrchestrator = calibrationOrchestrator;
        }

        /// <summary>
        /// Starts calibration
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="500">Internal error or configuration not found</response>
        [HttpPost("start")]
        public void StartCalibration()
        {
            calibrationOrchestrator.StartCalibration();
        }

        /// <summary>
        /// Cancel calibration
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="500">Internal error or configuration not found</response>
        [HttpPost("cancel")]
        public void CancelCalibration()
        {
            calibrationOrchestrator.CancelCalibration();
        }

        /// <summary>
        /// Sends calibration status
        /// </summary>
        [HttpGet("status")]
        public void SendCalibrationStatus()
        {
            calibrationOrchestrator.SendCalibrationStatus();
        }

        /// <summary>
        /// Sends calibration data/status
        /// </summary>
        [HttpGet("data/status")]
        public IActionResult SendCalibrationDataStatus()
        {
            return Ok(value: calibrationOrchestrator.GetCalibrationDataStatus());
        }

        [HttpGet("data/download")]
        public IActionResult Download()
        {
            var t = ftp.Download("ftp://localhost/imagem.JPG", "spec", "spec");
            return Ok("ok ");
        }

        [HttpGet("data/upload")]
        public IActionResult Upload()
        {
            var r = ftp.Upload("imagem.JPG", "spec", "spec");
            return Ok("ok ");
        }

        [HttpGet("executa/{name}")]
        public IActionResult Executa(string name)
        {
            var path = Directory.GetCurrentDirectory()+ @"\ExecutavelExterno\alteraimagem.exe";
            string app = path;
            string param = name;
            var process = Process.Start(app, param);
            process.Dispose();
            return Ok("ok");
        }

        //envia a requisição para a api do Lucas tem que ser enviado com post pois 
        //o Lucas alertou que o formato do dado é json
        [HttpPost("sendrequest")]
        public async Task<string> SendRequest() {
            HttpClient cliente = new HttpClient();

            string response = await cliente.GetStringAsync("http://apivmis.devrobot.com.br/ApiEndereco/region/br");
            return  response;
        }

        [HttpGet("sendpost")]
        public async Task<string> SendPostRequest()
        {
            //string dadosPOST = "code=1";
            //dadosPOST = dadosPOST + "&errorCause=uaheuaehaueahuea";
            //dadosPOST = dadosPOST + "&result=200";
            //var dados = Encoding.UTF8.GetBytes(dadosPOST);
            //var requisicaoWeb = WebRequest.CreateHttp("http://localhost:1469/teste/teste");
            //requisicaoWeb.Method = "POST";
            //requisicaoWeb.ContentType = "application/x-www-form-urlencoded";
            //requisicaoWeb.ContentLength = dados.Length;
            //requisicaoWeb.UserAgent = "RequisicaoWebDemo";
            ////precisamos escrever os dados post para o stream
            //using (var stream = requisicaoWeb.GetRequestStream())
            //{
            //    stream.Write(dados, 0, dados.Length);
            //    stream.Close();
            //}
            //using (var resposta = requisicaoWeb.GetResponse())
            //{
            //    var streamDados = resposta.GetResponseStream();
            //    StreamReader reader = new StreamReader(streamDados);
            //    object objResponse = reader.ReadToEnd();
            //    var post = objResponse.ToString();
            //    return post;
            //    streamDados.Close();    
            //    resposta.Close();
            //}

            //########################################################
            //var data = "code=1&errorCause=uaheuhuehauh&result=200";

            //Dados data = new Dados
            //{
            //    code = 1,
            //    errorCause = "auheueah",
            //    result = 200
            //};

            //HttpClient client = new HttpClient();
            //StringContent queryString = new StringContent(data);

            //HttpResponseMessage response = await client.PostAsync(new Uri("http://localhost:1469/teste/teste"), queryString);

            ////response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();

            //return responseBody;
            //########################################################
            
            //usando json
            var j = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

            using (var client = new HttpClient())
            {
                
                //client.BaseAddress = new Uri("http://apivmis.devrobot.com.br/ApiEndereco/region/br");
                var msg = new Msg();
                msg.code = 1;
                msg.result = 200;
                msg.errorCause = "teste da api auheueahe";

                var result = await client.PostAsJsonAsync(@"http://localhost:1468/teste/teste", msg);
                return result.ToString();

            }

        }


    }

    public class Dados {
        public int code { get; set; }
        public string errorCause { get; set; }
        public int result { get; set; }

        public Dados()
        {
        }
    }
}
