using UnityEngine;

public static class DistributionSampling
{
    // Box-Muller method for gaussian sampling from uniform distributions
    public static float NextGaussian(float mean, float stddev)
    {
        // The method requires sampling from a uniform random of (0,1]
        float x1 = 1 - Random.Range(0f, 1f);
        float x2 = 1 - Random.Range(0f, 1f);

        float y1 = Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Cos(2.0f * Mathf.PI * x2);

        return y1 * stddev + mean;
    }

    public static float NextGaussian(float mean, float stddev, float min, float max)
    {
        if (min > max)
            Debug.LogWarning("Min is greater than max");

        if (min > mean + stddev || max < mean - stddev)
            Debug.LogWarning("Min/max bounds outside of sigma range");

        return Mathf.Clamp(NextGaussian(mean, stddev), min, max);
    }
}
