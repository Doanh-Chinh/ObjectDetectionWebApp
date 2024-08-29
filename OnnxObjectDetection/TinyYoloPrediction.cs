using Microsoft.ML.Data;

namespace OnnxObjectDetection
{
    public class TinyYoloPrediction : IOnnxObjectPrediction
    {
        [ColumnName("grid")]
        //[ColumnName("2395")] for testing my own model

        public float[] PredictedLabels { get; set; }
    }
}
