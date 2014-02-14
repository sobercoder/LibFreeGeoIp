using System.Net;
using Newtonsoft.Json;

namespace FreeGeoIp
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Location
	{
		[JsonProperty(PropertyName = "ip")]
		[JsonConverter(typeof(StringIPAddressConverter))]
		public IPAddress IpAddress { get; set; }

		[JsonProperty(PropertyName = "country_code")]
		public string CountryCode { get; set; }

		[JsonProperty(PropertyName = "country_name")]
		public string Country { get; set; }

		[JsonProperty(PropertyName = "region_code")]
		public string RegionCode { get; set; }

		[JsonProperty(PropertyName = "region_name")]
		public string Region { get; set; }

		[JsonProperty(PropertyName = "city")]
		public string City { get; set; }

		[JsonProperty(PropertyName = "zipcode")]
		public string ZipCode { get; set; }

		[JsonProperty(PropertyName = "latitude")]
		public double Latitude { get; set; }

		[JsonProperty(PropertyName = "longitude")]
		public double Longitude { get; set; }

		[JsonProperty(PropertyName = "metro_code")]
		public string MetroCode { get; set; }

		[JsonProperty(PropertyName = "areacode")]
		public string AreaCode { get; set; }
	}
}



using System;
using System.Net;
using Newtonsoft.Json;

namespace FreeGeoIp
{
	public class FreeGeoIpClient
	{
		private static readonly string _freeGeoIpBaseUrl = "http://www.freegeoip.net/json";

		public static Location GetLocation()
		{
			return GetLocation(null, null);
		}

		public static Location GetLocation(string ipAddressOrHostname)
		{
			return GetLocation(ipAddressOrHostname, null);
		}

		public static Location GetLocation(IWebProxy proxy)
		{
			return GetLocation(null, proxy);
		}
		
		public static Location GetLocation(string ipAddressOrHostname, IWebProxy proxy)
		{
			// keep a backup of the original proxy settings
			IWebProxy lastWebProxy = WebRequest.DefaultWebProxy;
			Location location = null;

			try
			{
				if (proxy != null)
				{
					// change the default proxy settings
					WebRequest.DefaultWebProxy = proxy;
				}

				using (WebClient webClient = new WebClient())
				{
					string requestUrl = _freeGeoIpBaseUrl;

					if (!string.IsNullOrWhiteSpace(ipAddressOrHostname))
					{
						requestUrl = requestUrl + "/" + Uri.EscapeUriString(ipAddressOrHostname);
					}

					string jsonContent = webClient.DownloadString(requestUrl);

					if (!string.IsNullOrWhiteSpace(jsonContent))
					{
						location = JsonConvert.DeserializeObject<Location>(jsonContent);
					}
				}
			}
			finally
			{
				// restore the original proxy settings
				WebRequest.DefaultWebProxy = lastWebProxy;
			}

			return location;
		}
	}
}



using System;
using System.Net;
using Newtonsoft.Json;

namespace FreeGeoIp
{
	public class StringIPAddressConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(IPAddress));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw new Exception(string.Format("Unexpected token parsing date. Expected String, got {0}.", reader.TokenType));
			}

			return IPAddress.Parse(reader.Value as string);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}



using System;
using System.Net;

namespace FreeGeoIp.Tests
{
	internal class Test
	{
		static void Main(String[] args)
		{
			try
			{
				// Inject proxy credentials for transport through secured network (if any)
				WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
				WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

				Location location = FreeGeoIpClient.GetLocation();
			}
			catch (WebException ex)
			{
				Console.WriteLine("[ERROR] Execution stopped at " + DateTime.Now);
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			Console.ReadKey(true);
		}
	}
}
