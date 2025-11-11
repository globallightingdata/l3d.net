using L3D.Net.Data;
using L3D.Net.XML.V0_11_0.Dto;
using System;

namespace L3D.Net.Mapper.V0_11_0;

public sealed class FaceAssignmentMapper : DtoMapperBase<FaceAssignment, FaceAssignmentDto>
{
    public static readonly FaceAssignmentMapper Instance = new();

    protected override FaceAssignment ConvertData(FaceAssignmentDto element) => element switch
    {
        FaceRangeAssignmentDto faceRangeAssignment => new FaceRangeAssignment
        {
            FaceIndexBegin = faceRangeAssignment.FaceIndexBegin,
            FaceIndexEnd = faceRangeAssignment.FaceIndexEnd,
            GroupIndex = faceRangeAssignment.GroupIndex
        },
        SingleFaceAssignmentDto singleFaceAssignment => new SingleFaceAssignment
        {
            GroupIndex = singleFaceAssignment.GroupIndex,
            FaceIndex = singleFaceAssignment.FaceIndex
        },
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };

    protected override FaceAssignmentDto ConvertData(FaceAssignment element) => element switch
    {
        FaceRangeAssignment faceRangeAssignment => new FaceRangeAssignmentDto
        {
            FaceIndexBegin = faceRangeAssignment.FaceIndexBegin,
            FaceIndexEnd = faceRangeAssignment.FaceIndexEnd,
            GroupIndex = faceRangeAssignment.GroupIndex
        },
        SingleFaceAssignment singleFaceAssignment => new SingleFaceAssignmentDto
        {
            GroupIndex = singleFaceAssignment.GroupIndex,
            FaceIndex = singleFaceAssignment.FaceIndex
        },
        _ => throw new ArgumentOutOfRangeException(nameof(element))
    };
}