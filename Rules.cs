namespace StatisticalProcessControl
{
    public class Rules
    {
        /// <summary>
        /// Implements Nelson's SPC Rule 1, PointsBeyondStdDev.
        /// Returns the indices and values of points in a data set that are more than a specified number of standard deviations from the mean.
        /// </summary>
        /// <param name="data">The data set to analyze.</param>
        /// <param name="numStdDev">The number of standard deviations from the mean that a point must be to be considered a violation.</param>
        /// <param name="mean">The mean of the data set. If not provided, it will be calculated from the data set.</param>
        /// <param name="seedLimit">The index to start checking from. If not provided, it will default to 0.</param>
        /// <param name="stdDev">The standard deviation of the data set. If not provided, it will be calculated from the data set.</param>
        /// <returns>A list of tuples containing the indices and values of the points that violate the condition.</returns>
        public List<(int index, double value)> Rule1(double[] data, double numStdDev, double mean = 0, int seedLimit = 0,
            double stdDev = 0)
        {
            // Validate the input parameters
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data set cannot be null.");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Data set cannot be empty.", nameof(data));
            }

            if (numStdDev <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numStdDev),
                    "Number of standard deviations must be a positive number.");
            }

            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            if (mean < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mean), "Mean must be a non-negative number.");
            }

            if (stdDev < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard Deviation must be a non-negative number.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Calculate the mean and standard deviation if not provided
            if (mean == 0)
            {
                mean = data.Average();
            }

            if (stdDev == 0)
            {
                stdDev = StandardDeviation(data, mean);
            }

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length; i++)
            {
                // Check if the current point is more than the specified number of standard deviations from the mean
                double point = data[i];
                double distanceFromMean = Math.Abs(point - mean);
                if (distanceFromMean > numStdDev * stdDev)
                {
                    // Add the current point to the list of failed points
                    failedPoints.Add((i, point));
                    i = seedLimit - 1; // reset to start at seedLimit again
                }
            }

            return failedPoints;
        }

        /// <summary>
        /// Implements Nelson's SPC Rule 2, ConsecutivePointsOnSameSide.
        /// Returns a list of points in the given data set that are on the same side of the mean for a certain number of consecutive points, along with their indices in the original data set.
        /// </summary>
        /// <param name="data">The data set to analyze.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points that must be on the same side of the mean for a point to be considered an outlier.</param>
        /// <param name="seedLimit">The starting index in the data set to begin searching for outlier points. After each outlier point is found, the search will restart at this index.</param>
        /// <param name="mean">The mean of the data set. If not specified, the mean will be calculated from the data set.</param>
        /// <returns>A list of tuples containing the indices and values of the outlier points.</returns>
        public List<(int index, double value)> Rule2(double[] data, int numConsecutivePoints, int seedLimit,
            double mean = 0)
        {
            // Error handling
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data set cannot be null.");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Data set cannot be empty.", nameof(data));
            }

            if (numConsecutivePoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numConsecutivePoints),
                    "Number of consecutive points must be a positive integer.");
            }

            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            if (mean < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mean), "Mean must be a non-negative number.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Calculate mean if not provided
            if (mean == 0)
            {
                mean = data.Average();
            }

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numConsecutivePoints + 1; i++)
            {
                double point1 = data[i];
                bool sameSide = point1 < mean;
                bool fail = data.Skip(i + 1).Take(numConsecutivePoints - 1).All(point2 => (point2 < mean) == sameSide);
                if (fail)
                {
                    // Add the last point that violated the rule to the list of failed points
                    failedPoints.Add((i + numConsecutivePoints - 1, data[i + numConsecutivePoints - 1]));
                    i += numConsecutivePoints - 1; // skip the tested points
                }
            }

            return failedPoints;
        }

        /// <summary>
        /// Implements Nelson's SPC Rule 3, ConsecutivePointsIncreasingOrDecreasing
        /// Returns a list of points in the given data set that are continually increasing or decreasing for a certain number of consecutive points, along with their indices in the original data set.
        /// </summary>
        /// <param name="data">The data set to analyze.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points that must be continually increasing or decreasing for a point to be considered an outlier.</param>
        /// <param name="seedLimit">The starting index in the data set to begin searching for outlier points. After each outlier point is found, the search will restart at this index.</param>
        /// <returns>A list of tuples containing the indices and values of the outlier points.</returns>
        public List<(int index, double value)> Rule3(double[] data, int numConsecutivePoints, int seedLimit)
        {
            // Error handling
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data set cannot be null.");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Data set cannot be empty.", nameof(data));
            }

            if (numConsecutivePoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numConsecutivePoints),
                    "Number of consecutive points must be a positive integer.");
            }

            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numConsecutivePoints + 1; i++)
            {
                // Check if the sequence of consecutive points is continually increasing or decreasing
                bool increasing = data.Skip(i).Take(numConsecutivePoints)
                    .Zip(data.Skip(i + 1).Take(numConsecutivePoints - 1), (a, b) => a > b).All(x => x);
                bool decreasing = data.Skip(i).Take(numConsecutivePoints)
                    .Zip(data.Skip(i + 1).Take(numConsecutivePoints - 1), (a, b) => a < b).All(x => x);
                if (increasing || decreasing)
                {
                    // Add the last point that violated the rule to the list of failed points
                    failedPoints.Add((i + numConsecutivePoints - 1, data[i + numConsecutivePoints - 1]));
                    i += numConsecutivePoints - 1; // skip the tested points
                }
            }

            return failedPoints;
        }

        /// <summary>
        /// Implements Nelson's SPC Rule 4, ConsecutivePointsAlternating.
        /// Returns a list of points in the given data set that alternate in direction (increasing then decreasing) for a certain number of consecutive points, along with their indices in the original data set.
        /// </summary>
        /// <param name="data">The data set to analyze.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points that must alternate in direction for a point to be considered an outlier.</param>
        /// <param name="seedLimit">The starting index in the data set to begin searching for outlier points. After each outlier point is found, the search will restart at this index.</param>
        /// <returns>A list of tuples containing the indices and values of the outlier points.</returns>
        public List<(int index, double value)> Rule4(double[] data, int numConsecutivePoints, int seedLimit)
        {
            List<(int, double)> failedPoints = new List<(int, double)>();

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numConsecutivePoints + 1; i++)
            {
                // Compare each point to the next one and check if the comparison result alternates
                bool alternating = data
                    .Skip(i)
                    .Take(numConsecutivePoints)
                    .Zip(data.Skip(i + 1).Take(numConsecutivePoints - 1), (a, b) => a > b)
                    .Zip(Enumerable.Range(0, numConsecutivePoints - 1), (x, y) => x ^ (y % 2 == 0))
                    .All(x => x);
                if (alternating)
                {
                    // Add the last point that violated the rule to the list of failed points
                    failedPoints.Add((i + numConsecutivePoints - 1, data[i + numConsecutivePoints - 1]));
                    i += numConsecutivePoints - 1; // skip the tested points
                }
            }

            return failedPoints;
        }

        /// <summary>
        /// Implements Nelson's SPC Rule 5: XOutOfYPointsBeyondStdDev
        /// Returns a list of indices and values of points that are more than a specified number of standard deviations from the mean in the same direction for a specified number of consecutive points, out of a specified number of points.
        /// </summary>
        /// <param name="data">The data set to be analyzed.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points that must be more than the specified number of standard deviations from the mean in the same direction.</param>
        /// <param name="numOutOf">The total number of points to be considered in the analysis, including the consecutive points.</param>
        /// <param name="numStdDev">The number of standard deviations from the mean that the points must be.</param>
        /// <param name="mean">The mean of the data set. If not provided, the mean will be calculated from the data set.</param>
        /// <param name="seedLimit">The index at which to start the analysis. If not provided, the analysis will start from the beginning of the data set.</param>
        /// <param name="stdDev">The standard deviation of the data set. If not provided, the standard deviation will be calculated from the data set.</param>
        /// <returns>A list of tuples, each containing the index and value of the last point that violates the specified condition.</returns>
        public List<(int index, double value)> Rule5(double[] data, int numConsecutivePoints, int numOutOf, int numStdDev,
            double mean = 0, int seedLimit = 0, double stdDev = 0)
        {
            // Check for invalid numConsecutivePoints
            if (numConsecutivePoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numConsecutivePoints),
                    "Number of consecutive points must be a positive integer.");
            }

            // Check for invalid numOutOf
            if (numOutOf <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numOutOf),
                    "Number of points to be considered must be a positive integer.");
            }

            // Check for invalid numStdDev
            if (numStdDev <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numStdDev),
                    "Number of standard deviations must be a positive integer.");
            }

            // Check for invalid seedLimit
            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            if (mean < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mean), "Mean must be a non-negative number.");
            }

            if (stdDev < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard Deviation must be a non-negative number.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Calculate the standard deviation if not provided
            if (mean == 0)
            {
                mean = data.Average();
            }

            if (stdDev == 0)
            {
                stdDev = StandardDeviation(data, mean);
            }

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numOutOf + 1; i++)
            {
                // Check if the required number of consecutive points are more than the specified number of standard deviations from the mean in the same direction
                int numPointsAbove = data.Skip(i).Take(numOutOf).Count(x => x > mean + numStdDev * stdDev);
                int numPointsBelow = data.Skip(i).Take(numOutOf).Count(x => x < mean - numStdDev * stdDev);
                if (numPointsAbove >= numConsecutivePoints || numPointsBelow >= numConsecutivePoints)
                {
                    // Find the last point beyond the specified number of standard deviations from the mean in the consecutive points
                    int lastPointIndex = -1;
                    double lastPointValue = 0;
                    for (int j = i + numOutOf - 1; j >= i; j--)
                    {
                        double point = data[j];
                        if (point > mean + numStdDev * stdDev || point < mean - numStdDev * stdDev)
                        {
                            lastPointIndex = j;
                            lastPointValue = point;
                            break;
                        }
                    }

                    if (lastPointIndex != -1)
                    {
                        // Add the last point beyond the specified number of standard deviations from the mean to the list of failed points
                        failedPoints.Add((lastPointIndex, lastPointValue));
                    }

                    i += numOutOf - 1; // skip the tested points
                }
            }

            return failedPoints;
        }


        /// <summary>
        /// Implements Nelson's SPC rule 6
        /// Returns a list of indices and values of points that are more than a specified number of standard deviations from the mean in the same direction for a specified number of consecutive points, out of a specified number of points.
        /// </summary>
        /// <param name="data">The data set to be analyzed.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points that must be more than the specified number of standard deviations from the mean in the same direction.</param>
        /// <param name="numOutOf">The total number of points to be considered in the analysis, including the consecutive points.</param>
        /// <param name="numStdDev">The number of standard deviations from the mean that the points must be.</param>
        /// <param name="mean">The mean of the data set. If not provided, the mean will be calculated from the data set.</param>
        /// <param name="seedLimit">The index at which to start the analysis. If not provided, the analysis will start from the beginning of the data set.</param>
        /// <param name="stdDev">The standard deviation of the data set. If not provided, the standard deviation will be calculated from the data set.</param>
        /// <returns>A list of tuples, each containing the index and value of a point that violates the specified condition.</returns>
        public List<(int index, double value)> Rule6(double[] data, int numConsecutivePoints, int numOutOf, int numStdDev,
            double mean = 0, int seedLimit = 0, double stdDev = 0)
        {
            // Check for invalid numConsecutivePoints
            if (numConsecutivePoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numConsecutivePoints),
                    "Number of consecutive points must be a positive integer.");
            }

            // Check for invalid numOutOf
            if (numOutOf <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numOutOf),
                    "Number of points to be considered must be a positive integer.");
            }

            // Check for invalid numStdDev
            if (numStdDev <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numStdDev),
                    "Number of standard deviations must be a positive integer.");
            }

            // Check for invalid seedLimit
            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            if (mean < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mean), "Mean must be a non-negative number.");
            }

            if (stdDev < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard Deviation must be a non-negative number.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Calculate the standard deviation if not provided
            if (mean == 0)
            {
                mean = data.Average();
            }

            if (stdDev == 0)
            {
                stdDev = StandardDeviation(data, mean);
            }

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numOutOf + 1; i++)
            {
                // Check if the required number of consecutive points are more than the specified number of standard deviations from the mean
                int numPointsAbove = data.Skip(i).Take(numOutOf).Count(x => x > mean + numStdDev * stdDev);
                int numPointsBelow = data.Skip(i).Take(numOutOf).Count(x => x < mean - numStdDev * stdDev);
                if (numPointsAbove >= numConsecutivePoints || numPointsBelow >= numConsecutivePoints)
                {
                    // Find the last point beyond the specified number of standard deviations from the mean in the consecutive points
                    int lastPointIndex = -1;
                    double lastPointValue = 0;
                    for (int j = i + numOutOf - 1; j >= i; j--)
                    {
                        double point = data[j];
                        if (point > mean + numStdDev * stdDev || point < mean - numStdDev * stdDev)
                        {
                            lastPointIndex = j;
                            lastPointValue = point;
                            break;
                        }
                    }

                    if (lastPointIndex != -1)
                    {
                        failedPoints.Add((lastPointIndex, lastPointValue));
                    }

                    i += numOutOf - 1; // skip the tested points
                }
            }

            return failedPoints;
        }

        /// <summary>
        /// Implements Nelson's SPC Rule 7
        /// Returns a list of indices and values of points that are not within a specified number of standard deviations of the mean for a specified number of consecutive points, starting from a specified index in a given data set.
        /// </summary>
        /// <param name="data">The data set to be analyzed.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points to be checked for being within the specified number of standard deviations of the mean.</param>
        /// <param name="numStdDev">The number of standard deviations from the mean that the points must be within.</param>
        /// <param name="mean">The mean of the data set. If not provided, the mean will be calculated from the data set.</param>
        /// <param name="seedLimit">The index at which to start the analysis. If not provided, the analysis will start from the beginning of the data set.</param>
        /// <param name="stdDev">The standard deviation of the data set. If not provided, the standard deviation will be calculated from the data set.</param>
        /// <returns>A list of tuples, each containing the index and value of the last point that violates the specified condition.</returns>
        public List<(int index, double value)> Rule7(double[] data, int numConsecutivePoints, double numStdDev,
            double mean = 0, int seedLimit = 0, double stdDev = 0)
        {
            // Check for null or empty data set
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("Data set cannot be null or empty.", nameof(data));
            }

            // Check for invalid numConsecutivePoints
            if (numConsecutivePoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numConsecutivePoints),
                    "Number of consecutive points must be a positive integer.");
            }

            // Check for invalid numStdDev
            if (numStdDev <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numStdDev),
                    "Number of standard deviations must be a positive number.");
            }

            // Check for invalid seedLimit
            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            if (mean < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mean), "Mean must be a non-negative number.");
            }

            if (stdDev < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard Deviation must be a non-negative number.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Calculate the mean and standard deviation if not provided
            if (mean == 0)
            {
                mean = data.Average();
            }

            if (stdDev == 0)
            {
                stdDev = StandardDeviation(data, mean);
            }

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numConsecutivePoints + 1; i++)
            {
                // Check if the required number of consecutive points are all within the specified number of standard deviations from the mean on either side of the mean
                bool fail = data.Skip(i).Take(numConsecutivePoints)
                    .All(x => x < mean + numStdDev * stdDev && x > mean - numStdDev * stdDev);
                if (fail)
                {
                    // Add the last point that violates the condition to the list of failed points
                    failedPoints.Add((i + numConsecutivePoints - 1, data[i + numConsecutivePoints - 1]));
                    i = seedLimit - 1; // reset to start at seedLimit again
                }
            }

            return failedPoints;
        }

        /// <summary>
        /// Implements Nelson's SPC Rule 8.
        /// Returns all points in a row where none are within the specified number of standard deviations from the mean, and the points are in both directions from the mean.
        /// </summary>
        /// <param name="data">The data set to be analyzed.</param>
        /// <param name="numConsecutivePoints">The number of consecutive points in the row.</param>
        /// <param name="numStdDev">The number of standard deviations from the mean that the points must be outside of.</param>
        /// <param name="mean">The mean of the data set. If not provided, it will be calculated from the data set.</param>
        /// <param name="seedLimit">The index to start checking for failed points from. If not provided, it will default to 0.</param>
        /// <param name="stdDev">The standard deviation of the data set. If not provided, it will be calculated from the data set.</param>
        /// <returns>A list of tuples containing the index and value of the failed points.</returns>
        public List<(int index, double value)> Rule8(double[] data, int numConsecutivePoints, double numStdDev,
            double mean = 0, int seedLimit = 0, double stdDev = 0)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data set cannot be null.");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Data set cannot be empty.", nameof(data));
            }

            if (numConsecutivePoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numConsecutivePoints),
                    "Number of consecutive points must be a positive integer.");
            }

            if (numStdDev <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numStdDev),
                    "Number of standard deviations must be a positive number.");
            }

            if (seedLimit < 0 || seedLimit >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(seedLimit),
                    "Seed limit must be a non-negative integer less than the length of the data set.");
            }

            if (mean < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mean), "Mean must be a non-negative number.");
            }

            if (stdDev < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard Deviation must be a non-negative number.");
            }

            List<(int, double)> failedPoints = new List<(int, double)>();

            // Calculate the mean and standard deviation if not provided
            if (mean == 0)
            {
                mean = data.Average();
            }

            if (stdDev == 0)
            {
                stdDev = StandardDeviation(data, mean);
            }

            // Iterate through the data array starting from the seed limit
            for (int i = seedLimit; i < data.Length - numConsecutivePoints + 1; i++)
            {
                // Check if there are no points within the specified number of standard deviations from the mean
                bool fail = data.Skip(i).Take(numConsecutivePoints)
                    .All(x => x < mean - numStdDev * stdDev || x > mean + numStdDev * stdDev);
                if (fail)
                {
                    // Add the last point that violates the condition to the list of failed points
                    failedPoints.Add((i + numConsecutivePoints - 1, data[i + numConsecutivePoints - 1]));
                    i = seedLimit - 1; // reset to start at seedLimit again
                }
            }
            
            return failedPoints;
        }
        private double StandardDeviation(double[] data, double mean)
        {
            double sumOfSquares = data.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquares / (data.Length - 1));
        }
    }
}