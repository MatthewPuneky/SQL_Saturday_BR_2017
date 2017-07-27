using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class ParallelExamples
    {
        public static void Main(string[] args)
        {
            ForLoop();
            //ParallelForLoop();
            //HowItWorks();
            //ParallelForeachLoop();
            //DegreeOfParallelism();
            //CocurrentSearch();
            //ParallelSearch();
            //Locks();
            //Deadlocks();
            //MultipleResultsWithConcurrentBag();
            //MultipleResultsWithLinq();
            //MultipleResultsWithLock();
            //TwoDifferentFunctionsParallel();

            Console.ReadLine();
        }

        private static void ForLoop()
        {
            var stopwatch = Stopwatch.StartNew();

            stopwatch.Start();
            for (var i = 0; i < 100; i++)
            {
                Thread.Sleep(10);
            };
            stopwatch.Stop();

            Console.WriteLine($"Concurrent: {stopwatch.ElapsedMilliseconds}");
        }

        private static void ParallelForLoop()
        {
            var stopwatch = Stopwatch.StartNew();

            stopwatch.Start();
            Parallel.For(0, 100, i =>
            {
                Thread.Sleep(10);
            });
            stopwatch.Stop();

            Console.WriteLine($"Parallel: {stopwatch.ElapsedMilliseconds}");
        }

        private static void HowItWorks()
        {
            Parallel.For(0, 20, i =>
            {
                Console.WriteLine($"I am operating on index {i}");
                Thread.Sleep(5000);
            });
        }

        private static void ParallelForeachLoop()
        {
            var listOfStrings = new List<string> {"zero", "one", "two", "three", "four", "five", "six"};

            Parallel.ForEach(listOfStrings, aStringFromTheList =>
            {
                Console.WriteLine($"I am operating on list item {aStringFromTheList}");
                Thread.Sleep(5000);
            });
        }

        private static void CocurrentSearch()
        {
            var animals = new[] { "cat", "dog", "bird", "frog", "horse", "spider", "lizard",
                "pony", "fly", "zebra", "otter", "bear" };
            var found = string.Empty;

            foreach (var animal in animals)
            {
                Console.WriteLine($"Does {animal} == bird?");
                if (animal == "bird")
                {
                    found = animal;
                    break;
                }
                Thread.Sleep(500);
            }

            Console.WriteLine($"Found {found}");
        }

        private static void DegreeOfParallelism()
        {
            string[] animals = { "cat", "dog", "bird", "frog", "horse", "spider", "lizard" };

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 2
            };

            Parallel.ForEach(animals, options, animal =>
            {
                Console.WriteLine($"{animal} with {options.MaxDegreeOfParallelism} tasks operating in parallel");
                Thread.Sleep(1000);
            });
        }

        private static void ParallelSearch()
        {
            var animals = new [] { "cat", "dog", "bird", "frog", "horse", "spider", "lizard",
                "pony", "fly", "zebra", "otter", "bear" };

            var loopResult = Parallel.ForEach(animals, (animal, loop) =>
            {
                Console.WriteLine($"Does {animal} == spider?");
                if (animal == "spider")
                {
                    loop.Break();
                }
                Thread.Sleep(500);
            });
            
            if (loopResult.IsCompleted)
            {
                Console.WriteLine("Loop completed!");
            }
            else if (loopResult.IsCompleted == false
                    && loopResult.LowestBreakIteration.HasValue == false)
            {
                Console.WriteLine("Stop was used to exit the loop early...");
            }
            else if (loopResult.IsCompleted == false
                    && loopResult.LowestBreakIteration.HasValue)
            {
                var indexOfFound = loopResult.LowestBreakIteration.Value;
                Console.WriteLine($"We found the value {animals[indexOfFound]} at index: {indexOfFound}");
            }
        }

        private static void Locks()
        {
            string[] animals = { "cat", "dog", "bird", "frog", "horse", "spider", "lizard" };

            var unsafeList = new List<string>();
            var listLock = new object();

            Parallel.ForEach(animals, animal =>
            {
                lock (listLock)
                {
                    Thread.Sleep(1000);
                    unsafeList.Add(animal);
                    Console.WriteLine($"Added: {animal}");
                }
            });
        }

        private static void Deadlocks()
        {
            var FIRSTLOCK = new object();
            var secondlock = new object();

            Parallel.Invoke(
                () =>
                {
                    lock (FIRSTLOCK)
                    {
                        Console.WriteLine("T1: locked firstLock");
                        Thread.Sleep(50);

                        lock (secondlock)
                        {
                            Console.WriteLine("T1: locked secondLock");
                        }
                    }
                },
                () =>
                {
                    lock (secondlock)
                    {
                        Console.WriteLine("T2: locked secondLock");
                        Thread.Sleep(50);

                        lock (FIRSTLOCK)
                        {
                            Console.WriteLine("T2: locked firstLock");
                        }
                    }
                });
        }
        
        private static void TwoDifferentFunctionsParallel()
        {
            string[] animals = { "cat", "dog", "bird", "frog", "horse", "spider", "lizard" };
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

            var options = new ParallelOptions();
            var source = new CancellationTokenSource();
            
            options.CancellationToken = source.Token;

            Parallel.Invoke(options, 
                () =>
                {
                    for (var i = 0; i < animals.Length; i++)
                    {
                        queue.Enqueue(animals[i]);
                        Thread.Sleep(500);
                    }

                    source.Cancel();
                },
                () => 
                {
                    while (!options.CancellationToken.IsCancellationRequested)
                    {
                        string curVal;
                        if (queue.TryDequeue(out curVal))
                        {
                            Console.WriteLine(curVal);
                        }

                        Thread.Yield();
                    }
                }
            );

            source.Dispose();
        }

        private static void MultipleResultsWithLinq()
        {
            var animals = new List<string> { "cat", "dog", "bird", "frog", "horse", "spider", "lizard" };

            var result = animals.AsParallel().Where(s => s.Length == 4);

            foreach (var animal in result)
            {
                Console.WriteLine(animal);
            }
        }

        private static void MultipleResultsWithConcurrentBag()
        {
            var animals = new[] { "cat", "dog", "bird", "frog", "horse", "spider", "lizard" };
            var resultCollection = new ConcurrentBag<string>();

            Parallel.ForEach(animals, animal =>
            {
                if (animal.Length == 4)
                {
                    resultCollection.Add(animal);
                }
            });

            foreach (var result in resultCollection)
            {
                Console.WriteLine(result);
            }
        }

        private static void MultipleResultsWithLock()
        {
            var animals = new[] { "cat", "dog", "bird", "frog", "horse", "spider", "lizard" };
            var resultCollection = new List<string>();
            object localLockObject = new object();

            Parallel.For(0, animals.Length, () => new List<string>(), (i, state, localList) =>
            {
                if (animals[i].Length == 4)
                {
                    localList.Add(animals[i]);
                }
                return localList;
            },
            (finalResult) => { lock (localLockObject) resultCollection.AddRange(finalResult); });

            foreach (var result in resultCollection)
            {
                Console.WriteLine(result);
            }
        }

        private static void Exceptions()
        {
            //var exceptions = new ConcurrentQueue<>();
        }
    }
}
