Eve.NET [![Build status](https://ci.appveyor.com/api/projects/status/gvd1i4565flupru6?svg=true)](https://ci.appveyor.com/project/nicolaiarocci/eve-net)
=======
Eve.NET 是一个简单的用于 [Eve 框架][1] 生成的 Web 服务的 HTTP 和 REST 客户端。它同时使用 `System.Net.HttpClient` 和 `Json.NET` 来提供 .NET 平台最好的 Eve 体验。

跨平台
--------------
Eve.NET 作为 .NET Standard 1.1 包分发，这使它兼容于 [大范围][10] 操作系统和 .NET 平台。

用法
-----

### 初始化
```C#
// 可能的最简单的初始化.
var client = new EveClient();
client.BaseAddress = new Uri("http://api.com");

// 或
var client = new EveClient { BaseAddress = new Uri("http://api.com") };

// 或!
var client = new EveClient { 
    BaseAddress = new Uri("http://api.com"), 
    BasicAuthenticator = new BasicAuthenticator  ("user", "pw")
};

// 为下一步请求设置目标资源.
client.ResourceName = "companies";
````
### 在资源终结点 GET
```C#
// 返回一个 List<T>.
var companies = await client.GetAsync<Company>();

Assert.AreEqual(HttpStatusCode.OK, client.HttpResponse.StatusCode);
Assert.AreEqual(companies.Count, 10);

// 返回包含某个 DateTime 之后改变项的 List<T>.
var ifModifiedSince = DateTime.Now.AddDays(-1);
var companies = await client.GetAsync<Company>(ifModifiedSince);

Assert.AreEqual(HttpStatusCode.OK, client.HttpResponse.StatusCode);
Assert.AreEqual(companies.Count, 2);
```
### 在文档终结点 GET
```C#
var company = companies[0];

// 通过悄悄进行一个基于对象 ETag 的 If-None-Match 请求来更新一个已经存在的对象.
// 参考 http://python-eve.org/features#conditional-requests
company = await client.GetAsync<Company>(company);

// StatusCode 是 'NotModified'，因为 ETag 匹配了服务器上的一个 (没有进行任何下载). 出现下载也没事. 对象未变化. 
Assert.AreEqual(HttpStatusCode.NotModified, client.HttpResponse.StatusCode);


// 原生，带条件的 GET 请求
var companyId = "507c7f79bcf86cd7994f6c0e";
var eTag = "7776cdb01f44354af8bfa4db0c56eebcb1378975";

var company = await client.GetAsync<Company>("companies", companyId, eTag);

// HttpStatusCode i仍是 'NotModified'.
Assert.AreEqual(HttpStatusCode.NotModified, client.HttpResponse.StatusCode);
```
### POST/创建请求
```C#
var company = await client.PostAsync<Company>(new Company { Name = "MyCompany" });

// HttpStatusCode 是 'Created'.
Assert.AreEqual(HttpStatusCode.Created, client.HttpResponse.StatusCode);
Assert.AreEqual("MyCompany", company.Name);

// 新建的对象包含正确初始化的 API 元数据字段.
Assert.IsInstanceOf<DateTime>(company.Created);
Assert.IsInstanceOf<DateTime>(company.Updated);
Assert.IsNotNullOrEmpty(company.UniqueId);
Assert.IsNotNullOrEmpty(company.ETag);
```
### PUT/替代请求
```C#
company.Name = "YourCompany";

// PUT 请求将悄悄进行一个 If-Match 请求，因此，如果服务器和文档 ETag 匹配，只是更新服务器副本.
// 参考 http://python-eve.org/features#data-integrity-and-concurrency-control
var result = await client.PutAsync<Company>(company);

Assert.AreEqual(HttpStatusCode.OK, client.HttpResponse.StatusCode);
Assert.AreEqual(result.Name, company.Name);

// UniqueId 和 Created 没有变化.
Assert.AreEqual(result.UniqueId, company.UniqueId);
Assert.AreEqual(result.Created, company.Created);

// 但是 Updated 和 ETag 已经更新了.
Assert.AreNotEqual(result.Updated, company.Updated);
Assert.AreNotEqual(result.ETag, company.ETag);
```
### DELETE 请求
```C#
// DELETE 请求将悄悄进行一个 If-Match 请求，因此，如果它的 ETag 匹配了服务器上的某一项，只是删除文档.
// 参考 http://python-eve.org/features#data-integrity-and-concurrency-control
var message = await client.DeleteAsync(Original);
Assert.AreEqual(HttpStatusCode.OK, message.StatusCode);
```
### 映射 JSON 和 Eve 元数据字段到类成员
```C#
// 通过标识 RemoteAttribute，你可以映射 Eve 元数据字段到类属性. 因为这些在 API 终结点之间常常是不变的，
// 所以为你的业务对象提供一个基类很可能是个好主意.
public abstract class BaseClass
{
    [JsonProperty("_id")]
    [Remote(Meta.DocumentId)]
    public string UniqueId { get; set; }

    [JsonProperty("_etag")]
    [Remote(Meta.ETag)]
    public string ETag { get; set; }

    [JsonProperty("_updated")]
    [Remote(Meta.LastUpdated)]
    public DateTime Updated { get; set; }

    [JsonProperty("_created")]
    [Remote(Meta.DateCreated)]
    public DateTime Created { get; set; }
}

默认情况下，类与 json 的序列化/反序列化会采用 snake_case 转换。例如，在 json 中 `MyClass.ThisIsAProperty` 会被序列化到`this_is_a_property`。如果你想改变这个行为，设置对应的 `ContractResolver` 属性。
```
### 原生 GET 请求
```C#
// 你可以使用这个方法来进行参数化的查询.
var query = @"companies?where={""n"": ""MyCompany""}";
// GetAsync 会返回一个你可以轻松查看的 HttpResponseMessage.
var response = await client.GetAsyc(query);

Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
Assert.AreEqual("application/json", result.Content.Headers.ContentType.ToString())

// 请注意，使用 GetAsync<T> 你也获取到了通过 HttpResponse 属性暴露的 HttpResponseMessage 对象.
```
安装
------------
Eve.NET 已放在 [NuGet][9] 上。在包管理器控制台运行如下命令:

```
PM> Install-Package Eve.NET
```
或者通过 Visual/Xamarin Studio 中的 NuGet 包管理器安装。 

运行测试
-----------------
你应该 clone [`evenet-testbed`][7] 代码库，运行测试 Web 服务的一个本地实例。要不然，如果你手里没有一个 Python/Eve 环境的话，你可以选择依靠一个在线可用的免费 (而且慢，还资源有限) 测试实例。查看 [tests code][8] 获取详细信息。

许可证
-------
Eve.NET 是一个 [Nicola Iarocci][2] 和 [Gestionali Amica][3] 的开源项目，基于 [BSD 许可证][4] 分发。

[1]: http://python-eve.org
[2]: http://nicolaiarocci.com
[3]: http://gestionaleamica.com
[4]: https://github.com/pyeve/Eve.NET/blob/master/LICENSE.txt
[5]: http://msdn.microsoft.com/en-us/library/system.net.http.httpclient%28v=vs.118%29.aspx
[6]: http://james.newtonking.com/json
[7]: https://github.com/pyeve/Eve.NET-testbed
[8]: https://github.com/pyeve/Eve.NET/blob/master/Eve.Client.Tests/MethodsBase.cs#L13-L31
[9]: https://www.nuget.org/packages/Eve.NET/
[10]: https://docs.microsoft.com/en-us/dotnet/standard/net-standard
