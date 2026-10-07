namespace EngineeringApp.Shared.Models;

/// <summary>
/// 截面受力荷载工况
/// </summary>
public class LoadCase
{
    /// <summary>轴向力 N (kN, 受拉为正，受压为负)</summary>
    public double AxialForceN { get; set; } = -200.0;

    /// <summary>绕水平形心轴弯矩 My (kN·m, 使上边缘受压为正)</summary>
    public double BendingMomentMy { get; set; } = 50.0;

    /// <summary>绕竖向形心轴弯矩 Mz (kN·m, 使右边缘受压为正)</summary>
    public double BendingMomentMz { get; set; } = 15.0;

    /// <summary>水平向剪力 Vy (kN)</summary>
    public double ShearForceVy { get; set; } = 0.0;

    /// <summary>竖向剪力 Vz (kN)</summary>
    public double ShearForceVz { get; set; } = 30.0;

    /// <summary>自由扭矩 T (kN·m)</summary>
    public double TorsionT { get; set; } = 0.0;

    /// <summary>是否开启荷载应力验算</summary>
    public bool EnableStressCheck { get; set; } = true;
}
