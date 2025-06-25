using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Experiment : ScriptableObject
{
    public enum UpdateMethod
    {
        NormalUpdate,
        FixedUpdate,
        Manual
    }

    #region ExperimentVal Classes

    public abstract class ExperimentVal
    {
        public abstract int NumValues
        {
            get;
        }

        public abstract ExperimentVal GetValue(int index);

        public T GetValue<T>()
        {
            var val = GetValue(0) as SingleVal<T>;
            if (val == null)
                throw new System.Exception("Tried to get ExperimentValue of wrong type!");
            else
                return val.Value;
        }
    }

    public class SingleVal<T> : ExperimentVal
    {
        public SingleVal()
        {

        }

        public SingleVal(T val)
        {
            Value = val;
        }

        public T Value { get; set; }
        public override int NumValues { get { return 1; } }

        public override ExperimentVal GetValue(int index)
        {
            return new SingleVal<T>(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class FloatRange : ExperimentVal
    {
        public FloatRange()
        {

        }

        public FloatRange(float min, float max, int count)
        {
            Min = min;
            Max = max;
            Count = count;
        }

        public float Min { get; set; }
        public float Max { get; set; }
        public int Count { get; set; }

        public override int NumValues
        {
            get
            {
                return Count;
            }
        }

        public override ExperimentVal GetValue(int index)
        {
            if (Max < Min)
                throw new System.Exception("Experiment DoubleRange with Max < Min!");
            if (index < 0 || index >= Count)
                throw new System.Exception("Tried to get an invalid experiment value");

            double range = Max - Min;
            double ratio = (double)index / (double)(Count - 1);

            return new SingleVal<float>((float)(Min + range * ratio));

        }

        public override string ToString()
        {
            return $"Min: {Min} Max: {Max} Count: {Count}";
        }
    }

    public class DoubleRange : ExperimentVal
    {
        public DoubleRange()
        {

        }

        public DoubleRange(double min, double max, int count)
        {
            Min = min;
            Max = max;
            Count = count;
        }

        public double Min { get; set; }
        public double Max { get; set; }
        public int Count { get; set; }

        public override int NumValues
        {
            get
            {
                return Count;
            }
        }

        public override ExperimentVal GetValue(int index)
        {
            if (Max < Min)
                throw new System.Exception("Experiment DoubleRange with Max < Min!");
            if (index < 0 || index >= Count)
                throw new System.Exception("Tried to get an invalid experiment value");

            double range = Max - Min;
            double ratio = (double)index / (double)(Count - 1);

            return new SingleVal<double>(Min + range * ratio);

        }

        public override string ToString()
        {
            return $"Min: {Min} Max: {Max} Count: {Count}";
        }
    }

    public class IntRange : ExperimentVal
    {
        public IntRange()
        {

        }

        public IntRange(int from, int to)
        {
            From = from;
            To = to;
        }

        public int From { get; set; }
        public int To { get; set; }

        public override int NumValues
        {
            get
            {
                return Mathf.Max((To - From) + 1, 0);
            }
        }

        public override ExperimentVal GetValue(int index)
        {
            int count = NumValues;

            if (From > To)
                throw new System.Exception("Experiment IntRange with From > To!");
            if (index < 0 || index >= count)
                throw new System.Exception("Tried to get an invalid experiment value");

            return new SingleVal<int>(From + index);

        }

        public override string ToString()
        {
            return $"From: {From} To: {To}";
        }
    }

    public class ExpValSet<T> : ExperimentVal
    {
        public ExpValSet() { }

        public ExpValSet(T[] vals)
        {

        }

        public List<T> Values { get; set; }

        public override int NumValues
        {
            get
            {
                if (Values == null)
                    return 0;

                return Values.Count;
            }
        }

        public override ExperimentVal GetValue(int index)
        {
            if (Values == null || index < 0 || index >= Values.Count)
                throw new System.Exception("Tried to get an invalid experiment value");

            return new SingleVal<T>(Values[index]);
        }

        public override string ToString()
        {
            return $"StringSet with {Values.Count} values";
        }
    }
    #endregion

    public bool ReloadSceneEveryTrial = true;

    public string SessionName 
    {
        get { return _session; }
    }

    public string BlockName
    {
        get { return _block; }
    }

    public int TrialNum
    {
        get { return _trialNum; }
    }

    public abstract bool Initialized { get; }
    public abstract bool Complete { get; }

    public abstract TrialSettings ParseTrialSettings(Dictionary<string, ExperimentVal> settings);
    public abstract string GetScenePath(TrialSettings settings);
    public abstract IEnumerator Initialize(TrialSettings settings, string sessionName, string blockName, int trialNum);
    public abstract void StartExperiment();
    public abstract bool UpdateExperiment(float deltaTime, float elapsedTime);
    public abstract void FinalizeExperiment();

    protected string _session;
    protected string _block;
    protected int _trialNum;



}
