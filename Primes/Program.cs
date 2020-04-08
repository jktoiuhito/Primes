using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace Primes
{
    /*
     * TODO:
     *  - parallelize creation of factor primes
     *  - compress cache-file
     */
    class Program
    {
        const string FILE = "primes.cache";
        const int INITIAL_CAPACITY = 1000000;
        const byte LIST_TAB_COUNT = 10;
        const byte LIST_TAB_PADDING = 10;

        static int Main()
        {
            var primes = new SortedList<long, long>(INITIAL_CAPACITY);
            
            //Try loading previous prime-list
            try
            {
                var serialized = File.ReadAllLines(FILE);
                foreach (var line in serialized)
                {
                    var prime = long.Parse(line);
                    primes.Add(prime, prime);
                }
                Console.WriteLine(
                    "Loaded cache of {0} primes from disk", primes.Count);
            }
            catch (Exception e)
            {
                primes.Add(2,2);
                primes.Add(3,3);
                Console.WriteLine(
                    "Couldn't load cache, starting from scratch ({0})",
                    e.Message);
            }

            // Wait for user to specify a number, then check if it is a prime
            string input;
            while ((input = Console.ReadLine()) != "exit")
            {
                // Print out primes with command 'list'
                if (input == "list")
                {
                    var tab = 0;
                    var line = new StringBuilder();
                    foreach (var prime in primes)
                    {
                        _ = line.Append(
                            prime.Key.ToString()
                            .PadRight(LIST_TAB_PADDING));
                        tab += 1;
                        if (tab >= LIST_TAB_COUNT)
                        {
                            Console.WriteLine(line);
                            _ = line.Clear();
                            tab = 0;
                        }
                    }
                    Console.WriteLine(
                        "Wrote out {0} prime numbers\n", primes.Count);
                    continue;
                }

                // Check that input really is a positive integer
                long integer;
                try
                {
                    integer = long.Parse(input);
                }
                catch (Exception)
                {
                    Console.WriteLine("Input must be an integer\n");
                    continue;
                }
                if (integer < 0)
                {
                    Console.WriteLine("Integer must be positive\n");
                    continue;
                }

                // Check if input is a prime.
                if (integer == 0)
                    Console.WriteLine("0 is not a prime.\n");
                else if (integer == 1)
                    Console.WriteLine("1 is a prime.\n");
                else if (integer % 2 == 0)
                    Console.WriteLine(
                        "{0} is not a prime (even number)\n", integer);
                else if (primes.Keys.Contains(integer))
                    Console.WriteLine("{0} is a prime (cached)\n", integer);
                else if (primes.Keys.Last() > integer)
                    Console.WriteLine(
                        "{0} is not a prime (cached)\n", integer);
                else
                {
                    /*
                     * All non-prime numbers can be formed by multiplication
                     * of prime numbers. Therefore, if a number is not
                     * divisible by any prime number, it itsel is a
                     * prime number.
                     * 
                     * The biggest prime factor that needs checking is
                     * sqrt(number), as x = y * y results in y = sqrt(x).
                     * 
                     * This algorithm first calculates all primes up to
                     * sqrt(number), and then tests that number % prime != 0
                     * for all primes less than sqrt(number).
                     */
                    var timer = Stopwatch.StartNew();
                    if (primes.Keys.Last() < (long)Math.Sqrt(integer))
                    {
                        var last = primes.Keys.Last();
                        var candidate = last;
                        while (last < (long)Math.Sqrt(integer))
                        {
                            candidate += 2;
                            if (IsPrime(candidate, primes))
                            {
                                primes.Add(candidate, candidate);
                                last = candidate;
                            }
                        }
                        timer.Stop();
                        Console.WriteLine(
                            "Creating potential factor primes " +
                            "finished in {0} seconds (currently {1} " +
                            "primes in cache)",
                            timer.ElapsedMilliseconds / 1000f,
                            primes.Count);
                    }
                    else
                        Console.WriteLine(
                            "Skip creating potential factor primes");
                    if (IsPrime(integer, primes))
                        Console.WriteLine("{0} is a prime\n", integer);
                    else
                        Console.WriteLine("{0} is not a prime\n", integer);
                }
            }

            // Save current list of primes
            try
            {
                var lines = new StringBuilder();
                foreach (var prime in primes)
                    _ = lines.AppendLine(prime.Key.ToString());
                File.WriteAllText(FILE, lines.ToString());
                Console.WriteLine(
                    "Wrote cache of {0} primes to disk", primes.Count);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Couldnt write cache to disk: {0}", e.Message);
                return 1;
            }
        }

        private static bool IsPrime(
            long candidate, SortedList<long, long> factorPrimes)
        {
            foreach (var prime in factorPrimes)
            {
                if (prime.Key > (long)Math.Sqrt(candidate))
                    return true;
                if (candidate % prime.Key == 0)
                    return false;
            }
            throw new Exception(
                "you shouldn't have been able to get here...");
        }
    }
}
