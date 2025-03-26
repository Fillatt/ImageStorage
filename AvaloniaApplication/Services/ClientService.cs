using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaApplication.Services;

/// <summary>Сервис для взаимодействия с Web-API.</summary>
public class ClientService
{
    #region Fields
    private string? _apiUrl;
    #endregion

    #region Properties
    public HttpClient HttpClient { get; set; } = new HttpClient();
    #endregion

    #region Constructors
    /// <summary>Инизиализирует экземпляр <see cref="ClientService"/>.</summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    public ClientService(IConfiguration configuration)
    {
        _apiUrl = configuration.GetConnectionString("WebApiUrl");
    }
    #endregion

    #region Public Methods
    /// <summary>Отправка изображения.</summary>
    /// <param name="uri"><see cref="Uri"/> изображения.</param>
    /// <exception cref="Exception"></exception>
    public async Task SendAsync(Uri uri)
    {
        var multipartFormContent = new MultipartFormDataContent();
        var fileStreamContent = new StreamContent(File.OpenRead(uri.LocalPath));
        fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        multipartFormContent.Add(fileStreamContent, name: "image", fileName: Path.GetFileName(uri.LocalPath));

        var response = await HttpClient.PostAsync($"{_apiUrl}/images/add", multipartFormContent);
        if (!response.IsSuccessStatusCode) throw new Exception();
    }

    /// <summary>Получение изображений.</summary>
    /// <returns>
    /// <see cref="Dictionary{TKey, TValue}"/>, содержащий элементы: 
    /// key - имя изображения,
    /// value - массив байтов изображения
    /// </returns>
    /// <exception cref="Exception"></exception>
    public async Task<Dictionary<string, byte[]>?> GetAsync()
    {
        var response = await HttpClient.GetAsync($"{_apiUrl}/images/all");
        if (!response.IsSuccessStatusCode) throw new Exception();
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<Dictionary<string, byte[]>>(content);
    }

    /// <summary>Обновляет изображение.</summary>
    /// <param name="index">Индекс изображения в списке.</param>
    /// <param name="uri"><see cref="Uri"/> изображения.</param>
    /// <exception cref="Exception"></exception>
    public async Task UpdateImageAsync(int index, Uri uri)
    {
        var multipartFormContent = new MultipartFormDataContent();
        var fileStreamContent = new StreamContent(File.OpenRead(uri.LocalPath));
        fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        multipartFormContent.Add(fileStreamContent, name: "image", fileName: Path.GetFileName(uri.LocalPath));

        var response = await HttpClient.PutAsync($"{_apiUrl}/images/update/{index + 1}", multipartFormContent);
        if (!response.IsSuccessStatusCode) throw new Exception();
    }

    /// <summary>Удаляет изображение.</summary>
    /// <param name="index">Индекс изображения в списке.</param>
    /// <exception cref="Exception"></exception>
    public async Task DeleteImageAsync(int index)
    {
        var response = await HttpClient.DeleteAsync($"{_apiUrl}/images/delete/{index + 1}");
        if (!response.IsSuccessStatusCode) throw new Exception();
    }
    #endregion
}
