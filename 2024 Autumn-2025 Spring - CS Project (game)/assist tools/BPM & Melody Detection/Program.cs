using Assist;
using System.Text;

string file = "C:\\Users\\user\\Desktop\\黃氏兄弟【不專心】首張單曲MV ｜ Official Music Video.mp3";

AudioHandler audioHandler = new(file);

//audioHandler.BPMDetect_Energy();
//audioHandler.BPMDetect_WeightedEnergy();
audioHandler.BPMDetect_Freq();
//audioHandler.GetMainMelodyOnset();

//List<float> test = new(new float[500]);
//for (int i = 0; i < 50; i++)
//{
//    test[i] = 1;
//}

//List<float> kernel = new(new float[50]);
//for (int i = 0; i < 50; i++)
//{
//    kernel[i] = 1 - (float)i / 50;
//}
//float sum = kernel.Sum();
//for (int i = 0; i < 50; i++)
//{
//    kernel[i] /= sum;
//}

//Queue<float> buffer = new(new float[50]);
//List<float> output = [];
//for (int i = 0; i < test.Count; i++)
//{
//    buffer.Enqueue(test[i]);
//    buffer.Dequeue();

//    float s = 0;
//    var temp = buffer.ToList();
//    for (int j = 0; j < kernel.Count; j++)
//    {
//        s += kernel[j] * temp[j];
//    }
//    output.Add(s);
//}

//buffer = new(new float[50]);
//List<float> reverse = [];
//foreach (var val in output)
//{
//    // compute the unblurred value
//    float s = 0;
//    var temp_buffer = buffer.ToList();
//    for (int i = 0; i < kernel.Count - 1; i++)
//    {
//        s += kernel[i] * temp_buffer[i + 1];
//    }
//    float value = (val - s) / kernel[^1];

//    // add to spectrum
//    if (value < 0) value = 0;

//    // push to buffer
//    buffer.Enqueue(value);
//    buffer.Dequeue();

//    reverse.Add(value);
//}

//StringBuilder stringBuilder = new StringBuilder();
//foreach (var val in output)
//{
//    stringBuilder.AppendLine(val.ToString());
//}
//File.WriteAllText("DEBUG__TEST.txt", stringBuilder.ToString());
//stringBuilder.Clear();
//foreach (var val in reverse[..^50])
//{
//    stringBuilder.AppendLine(val.ToString());
//}
//File.WriteAllText("DEBUG__TEST_REV.txt", stringBuilder.ToString());
//stringBuilder.Clear();