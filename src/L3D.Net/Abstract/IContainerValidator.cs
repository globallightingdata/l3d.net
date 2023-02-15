﻿namespace L3D.Net.Abstract;

public interface IContainerValidator
{
    bool Validate(string containerPath);

    bool Validate(byte[] containerBytes);
}