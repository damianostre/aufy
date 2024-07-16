namespace TestWebApi.IntegrationTests;

public class TestId
{
    private static int _counter = 0;
    private static readonly SemaphoreSlim _semaphoreSlim = new (1, 1);
    
    public int Current { get; set; }

    public TestId()
    {
        _semaphoreSlim.Wait();
        _counter++;
        _semaphoreSlim.Release();

        Current = _counter;
    }
    
    public string Format(string value)
    {
        return $"T{Current}_{value}";
    }
    
}