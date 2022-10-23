using System;
using Newtonsoft.Json;

namespace Loup.DotNet.Challenge.FunctionApp.Models
{
    public class Recipe : IEquatable<Recipe>
    {
        [JsonProperty("contentId")]
        public int ContentId { get; set; }

        [JsonProperty("contentType")]
        public int ContentType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("urlPartial")]
        public string UrlPartial { get; set; }

        [JsonProperty("servingSize")]
        public int ServingSize{ get; set; }

        [JsonProperty("energy")]
        public double Energy { get; set; }

        [JsonProperty("calories")]
        public double Calories { get; set; }

        [JsonProperty("Carbs")]
        public double Carbs { get; set; }

        [JsonProperty("Protein")]
        public double Protein { get; set; }

        [JsonProperty("DietryFibre")]
        public double DietryFibre { get; set; }

        [JsonProperty("Fat")]
        public double Fat { get; set; }

        [JsonProperty("SatFat")]
        public double SatFat { get; set; }

        [JsonProperty("Sugar")]
        public double Sugar { get; set; }

        //Compare two recipes together
        public bool Equals(Recipe other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ContentId == other.ContentId && ContentType == other.ContentType && Name == other.Name && Summary == other.Summary && UrlPartial == other.UrlPartial && ServingSize == other.ServingSize && Energy.Equals(other.Energy) && Calories.Equals(other.Calories) && Carbs.Equals(other.Carbs) && Protein.Equals(other.Protein) && DietryFibre.Equals(other.DietryFibre) && Fat.Equals(other.Fat) && SatFat.Equals(other.SatFat) && Sugar.Equals(other.Sugar);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Recipe)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ContentId);
            hashCode.Add(ContentType);
            hashCode.Add(Name);
            hashCode.Add(Summary);
            hashCode.Add(UrlPartial);
            hashCode.Add(ServingSize);
            hashCode.Add(Energy);
            hashCode.Add(Calories);
            hashCode.Add(Carbs);
            hashCode.Add(Protein);
            hashCode.Add(DietryFibre);
            hashCode.Add(Fat);
            hashCode.Add(SatFat);
            hashCode.Add(Sugar);
            return hashCode.ToHashCode();
        }
    }
}
