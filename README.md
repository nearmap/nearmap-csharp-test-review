# Nearmap C# Test (Code Review Version)

Instructions for the standard Nearmap C# test are provided below.

For this exercise, a full solution has been provided (there is no need to provide any code yourself). However, we would
like you to review the code so that we can have an in-depth technical discussion around the solution and how it can be
improved. The solution code in marked by `#region Solution Code` directives. All code outside of a region is part of the
initial problem statement.

## Nearmap C# Test

The purpose of this assignment is to test your familiarity with C#,
distributed systems concepts, performance benchmarking and TDD.

### Background

The source code that you are given is a very simple imitation of a key/value store:

* `DatabaseStore` represents a client to the central store that takes a while (500ms) to store and retrieve data.
* `DistributedCacheStore` represents a client to the distributed cache (eg. Redis) that takes much less time to turn
  around (100ms to store or retrieve).

This scenario is a simplified example of a typical high performance server cluster with a database, a distributed cache
and multiple worker nodes.

### Assumptions

After startup:

* Data in `DatabaseStore` never changes and can be cached forever.
* If `DatabaseStore.GetValue()` returns `null` for a key, the requested data item does not exist and will never exist.
* `DistributedCacheStore` is initially empty.

### Task

Complete the 2 parts below & submit the solution.
If the solution is incomplete, please state what hasn't been finished and outline how you are planning on solving it.

* Provided code can be modified at will.
* The whole solution must build with no errors.

#### Part 1

Implement the `IDataSource` interface to create a mechanism to retrieve data from `DatabaseStore` with lowest possible
latency. For a frequently-requested item your `IDataSource.GetValue()` implementation should have a better response time
than the distributed cache store (ie < 100ms).

* The user of the `IDataStore` interface must not have to deal with thread synchronisation.
* Write unit tests for the new `IDataSource` implementation (only), and ensure all tests pass.
* The solution should aim to minimise calls to the database.
* Your non-test code can use an IoC library if desired, and otherwise only .NET Framework or .NET Core
  libraries/classes.

#### Part 2

Complete the `Main()` to test your `IDataSource` implementation; it must:

* Populate `DatabaseStore` with the following data at startup:

      | key  | value  |
      |------|--------|
      | key0 | value0 |
      | key1 | value1 |
      | key2 | value2 |
      | key3 | value3 |
      | key4 | value4 |
      | key5 | value5 |
      | key6 | value6 |
      | key7 | value7 |
      | key8 | value8 |
      | key9 | value9 |

* Use 10 threads, each making 50 consecutive requests for a random key in the range (key0-key9). I.e. there would a
  total of 500 requests.
* For each request, print the managed ThreadId, requested key name, returned value, time to complete that request;
  similar to the following example:

      [1] Request 'key1', response 'value1', time: 50.05 ms
      [2] Request 'key2', response 'value2', time: 50.05 ms
