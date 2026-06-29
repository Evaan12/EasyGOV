using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Domain.Common;
using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public class BiometricEmbedding : ValueObject
    {
        public float[] VectorData { get; }

        private BiometricEmbedding() { VectorData = Array.Empty<float>(); }

        public BiometricEmbedding(float[] vectorData)
        {
            if (vectorData == null || vectorData.Length == 0)
                throw new DomainException("Biometric embedding vector cannot be empty.");

            if (vectorData.Length != 512)
                throw new DomainException("Invalid biometric embedding dimension. Expected exactly 512 dimensions.");

            VectorData = vectorData;
        }

        public string ToVectorString() => $"[{string.Join(",", VectorData.Select(v => v.ToString(CultureInfo.InvariantCulture)))}]";

        public double ComputeCosineSimilarity(BiometricEmbedding other)
        {
            if (other == null || other.VectorData.Length != 512) return 0;

            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < 512; i++)
            {
                dotProduct += VectorData[i] * other.VectorData[i];
                normA += VectorData[i] * VectorData[i];
                normB += other.VectorData[i] * other.VectorData[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var val in VectorData)
            {
                yield return val;
            }
        }
    }
}