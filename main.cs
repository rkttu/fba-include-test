#!/usr/bin/env dotnet
#:include ./include/native.cs

unsafe {
    const int N = 10;
    long* fib = (long*)Alloc(N * sizeof(long));
    try
    {
        long* p = fib;
        long* end = fib + N;

        *p++ = 0;
        *p++ = 1;
        for (; p < end; p++)
            *p = *(p - 1) + *(p - 2);

        for (p = fib; p < end; p++)
            WriteLine(*p);
    }
    finally
    {
        Free(fib);
    }
}
