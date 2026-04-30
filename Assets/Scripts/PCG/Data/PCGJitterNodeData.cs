using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Jitter Node", menuName = "PCG/Nodes/Jitter")]
public class PCGJitterNodeData : PCGNodeData
{
    [SerializeField] private float amountX = 0.5f;
    [SerializeField] private float amountY;
    [SerializeField] private float amountZ = 0.5f;

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Amount X", PCGParameterType.Float, amountX)
            {
                minValue = 0f, maxValue = 100f
            },
            new PCGNodeParameter("Amount Y", PCGParameterType.Float, amountY)
            {
                minValue = 0f, maxValue = 100f
            },
            new PCGNodeParameter("Amount Z", PCGParameterType.Float, amountZ)
            {
                minValue = 0f, maxValue = 100f
            }
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Amount X": amountX = (float)value; break;
            case "Amount Y": amountY = (float)value; break;
            case "Amount Z": amountZ = (float)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new List<PCGPoint>();
        if (inputPoints == null)
        {
            return output;
        }

        foreach (var point in inputPoints)
        {
            var modifiedPoint = point;
            modifiedPoint.position += new Vector3(
                context.GetRandomFloat(-amountX, amountX),
                context.GetRandomFloat(-amountY, amountY),
                context.GetRandomFloat(-amountZ, amountZ));

            output.Add(modifiedPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGJitterNodeView";
}
