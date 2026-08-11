namespace HybridWash.Services.Interfaces;

public interface ITokenGenerator
{
    string Generate(string id, string role, string fullName);
}
