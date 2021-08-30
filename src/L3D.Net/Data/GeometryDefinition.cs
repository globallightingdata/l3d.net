using System;
using System.IO;
using L3D.Net.API.Dto;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Data
{
    internal class GeometryDefinition
    {
        public string Id { get; }
        public IModel3D Model { get; }
        public GeometricUnits Units { get; }
        public string FileName => Path.GetFileName(Model.FilePath);

        public GeometryDefinition(string id, IModel3D model, GeometricUnits units)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(@"Value cannot be null or whitespace.", nameof(id));

            Id = id;
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Units = units;
        }
    }
}