# Nearmap C# Test (Code Review Version)

Instructions for the standard Nearmap C# test are provided below.

For this exercise, ***a full solution has been provided - there is no need to provide any code yourself***. However, we
would like you to review the code so that we can have an in-depth technical discussion around the solution and how it
can be improved. The solution code is marked by `#region Solution Code` directives. All code outside of a region is part
of the initial problem statement.

## Nearmap C# Test

The purpose of this assignment is to test your familiarity with C#, distributed systems concepts, performance
benchmarking and TDD.

### Background

The source code that you are given is a very simple imitation of a key/value store:

* `DatabaseStore` represents a client to the central store that takes a while (500ms) to store and retrieve data.
* `DistributedCacheStore` represents a client to the distributed cache (eg. Redis) that takes much less time to turn
  around (100ms to store or retrieve).

This scenario is a simplified example of a typical high performance server cluster with a database, a distributed cache
and multiple worker nodes.

### Assumptions and requirements

The implementation should follow the given assumptions and requirements, so take these into consideration when you do your review and provide feedback during the discussion:

* Data in `DatabaseStore` never changes and can be cached forever.
* If `DatabaseStore.GetValue()` returns `null` for a key, the requested data item does not exist and will never exist.
* `DistributedCacheStore` is initially empty.
* Data should be retrieved from `DatabaseStore` with lowest possible latency. For a frequently-requested item your `IDataSource.GetValue()` implementation should have a better response time than the distributed cache store (ie < 100ms).
* The user of the `IDataStore` interface must not have to deal with thread synchronisation.
* Sufficient unit test coverage for the `IDataSource` implementation.
* The solution should aim to minimise calls to the database.
* Use 10 threads, each making 50 consecutive requests for a random key in the range (key0-key9). I.e. there would a
  total of 500 requests.
* For each request, print the managed ThreadId, requested key name, returned value, time to complete that request;
  similar to the following example:
  
      [1] Request 'key1', response 'value1', time: 50.05 ms
      [2] Request 'key2', response 'value2', time: 50.05 ms
