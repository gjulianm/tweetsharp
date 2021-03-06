﻿
using System;
using System.Diagnostics;
using System.Net.Http;

namespace Tweetsharp
{
    public class HttpResponse
    {
        public HttpResponse(HttpResponseMessage response, Exception innerException, string errorMessage)
            : this(response, innerException)
        {
            ErrorMessage = errorMessage;
        }

        public HttpResponse(HttpResponseMessage response, Exception innerException)
            : this(response)
        {
            InnerException = innerException;
        }

        public HttpResponse(HttpResponseMessage response)
        {
            Response = response;
            SetStringContents();
        }

        public HttpResponse()
        {

        }

        [Conditional("DEBUG")] // Just needed for debugging. Of no purpose on production code.
        private async void SetStringContents()
        {
            if (Response != null && Response.Content != null)
            {
                var strContents = await Response.Content.ReadAsStringAsync();
                StringContents = strContents;
            }
        }

        public Exception InnerException { get; internal set; }
        public string StringContents { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public HttpResponseMessage Response { get; internal set; }

        public virtual bool Succeeded
        {
            get
            {
                return Response != null && Response.IsSuccessStatusCode;
            }
        }

        public static HttpResponse Default
        {
            get
            {
                return new HttpResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }
    }

    public class HttpResponse<T> : HttpResponse
    {
        public HttpResponse(T content, HttpResponseMessage response, Exception innerException, string errorMessage)
            : base(response, innerException)
        {
            Content = content;
        }

        public HttpResponse(T content, HttpResponseMessage response, Exception innerException)
            : base(response, innerException)
        {
            Content = content;
        }

        public HttpResponse(T content, HttpResponseMessage response)
            : base(response)
        {
            Content = content;
        }

        public HttpResponse(T content, HttpResponse response)
        {
            Content = content;
            ErrorMessage = response.ErrorMessage;
            InnerException = response.InnerException;
            Response = response.Response;
            StringContents = response.StringContents;
        }

        public T Content { get; internal set; }

        public override bool Succeeded
        {
            get
            {
                return base.Succeeded && Content != null;
            }
        }

        public static HttpResponse<T> BuildDefault(T content)
        {
            return new HttpResponse<T>(content, HttpResponse.Default);
        }
    }
}
