using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCarController : MonoBehaviour
{

    

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;
    public Transform frontLeftWheelModel;
    public Transform frontRightWheelModel;
    public Transform rearLeftWheelModel;
    public Transform rearRightWheelModel;
    public float speed = 0; //скорость авто текущее
    public float maxSpeed; //максимальная скорость на выбранной передаче
    public float currentSpeed; //текущая скорость
    public int minRevs = 750; //минимальное кол-во оборотов двигателя
    public int maxRevs = 6500; //максимальное кол-во оборотов двигателя
    public bool isAutomatic = false;
    public bool isRearWheel = true; //машина заднеприводная или да
    public List<Gears> gears;
    public float wheelCurrentRevs = 0; //обороты на валу
    public float engineCurrentRevs = 0; //обороты двигателя
    public bool isEngineStarted = false; //запущен ли двигатель
    public bool isHandBrake = true; //ручник блокирует задние колеса
    public int clutchPosition = 0; //насколько сильно выжато сцепление - 100 макс, 0 - отпущено
    public float maxSteeringAngle = 30; //максимальное значение выворота колес
    public float steeringWheelCoefficient = 14.7f;
    public float currentSteeringWheelAngle = 0;   
    public float currentSteeringAngle = 0; //текущее значение выворота колес
    public int currentGear = 1; //текущая передача
    public int brakeModifier = 2000; //сила прикладываемая при торможении
    

    // wheelCurrentRevs = engineCurrentRevs * (1-(clutchPosition * 0.01)); получаем выходное значение колес
    public void FixedUpdate()
    {
        //вызываем метод управления колес
        Steering();
        speed = this.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;

        float throttle = 0;
        if (Input.GetAxis("Vertical") >= 0)
        {
            OppositeBrake();
            throttle = Input.GetAxis("Vertical");

        } else
        {
            Brake();
        }
        //обороты двигателя
        engineCurrentRevs = minRevs + (throttle * (maxRevs - minRevs));
        //обороты колес
        wheelCurrentRevs = (engineCurrentRevs * (1 - (clutchPosition * 0.01f))) * gears[currentGear].ratio;
        //макс скорость на текущей передаче
        maxSpeed = ((0.7f * 3.14f) * (gears[currentGear].ratio / 10)) * (60f / 1000f) * (maxRevs * (1 - (clutchPosition * 0.01f)));
        //макс скорость для текущего кол-ва оборотов и текущей передачи
        currentSpeed = ((0.7f * 3.14f) * (gears[currentGear].ratio / 10)) * (60f / 1000f) * (engineCurrentRevs * (1 - (clutchPosition * 0.01f)));

        


        //определяем тип привода
        if (isRearWheel)
        {
            //если скорость не макс передаем обороты, если макс для выбранной передачи, то всё
            if (speed < currentSpeed)
            {
                rearLeftWheel.motorTorque = wheelCurrentRevs;
                rearRightWheel.motorTorque = wheelCurrentRevs;
            } else
            {
                rearLeftWheel.motorTorque = 0f;
                rearRightWheel.motorTorque = 0f;
            }
            
        } else
        {
            if (speed < currentSpeed)
            {
                frontLeftWheel.motorTorque = wheelCurrentRevs;
                frontRightWheel.motorTorque = wheelCurrentRevs;
            }
            else
            {
                frontLeftWheel.motorTorque = 0f;
                frontRightWheel.motorTorque = 0f;
            }
            
        }
        //определяем используемую логику для трансмиссии
        if (isAutomatic)
        {
            AutomaticTransmissionLogic();
        } else
        {
            MechanicTransmissionLogic();
        }
        //позиция колес
        ModelPosition();

    }
    public void MechanicTransmissionLogic()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            clutchPosition = 100;
            ClutchUp();
            clutchPosition = 0;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            clutchPosition = 100;
            ClutchDown();
            clutchPosition = 0;
        }
    }
    /// <summary>
    /// Логика АКПП
    /// </summary>
    public void AutomaticTransmissionLogic()
    {

        if(gears[currentGear].revsToGearUp < engineCurrentRevs)
        {
            clutchPosition = 100;
            ClutchUp();
            clutchPosition = 0;
        }
    }

    /// <summary>
    /// торможение
    /// </summary>
    public void Brake()
    {
        frontLeftWheel.brakeTorque = -(Input.GetAxis("Vertical")) * brakeModifier;
        frontRightWheel.brakeTorque = -(Input.GetAxis("Vertical")) * brakeModifier;
        if (isHandBrake) //если стоит ручник то усилие будет всегда константным на задние колеса
        {
            rearLeftWheel.brakeTorque = brakeModifier;
            rearRightWheel.brakeTorque = brakeModifier;
        }
        else
        {
            rearLeftWheel.brakeTorque = -(Input.GetAxis("Vertical")) * brakeModifier;
            rearRightWheel.brakeTorque = -(Input.GetAxis("Vertical")) * brakeModifier;
        }
    }
    //если педаль тормоза не нажата откатывает значения
    public void OppositeBrake()
    {
        frontLeftWheel.brakeTorque = 0;
        frontRightWheel.brakeTorque = 0;
        if (isHandBrake) //если стоит ручник то усилие будет всегда константным на задние колеса
        {
            rearLeftWheel.brakeTorque = brakeModifier;
            rearRightWheel.brakeTorque = brakeModifier;
        }
        else
        {
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }
    }
    /// <summary>
    /// Повышение передачи
    /// </summary>
    public void ClutchUp()
    {
        if(clutchPosition == 100)
        {
            if ((currentGear+1) < gears.Count)
            {
                currentGear++;
            }
        }
    }
    /// <summary>
    /// Понижение передачи
    /// </summary>
    public void ClutchDown()
    {
        if (clutchPosition == 100)
        {
            if ((currentGear-1) >= 0)
            {
                currentGear--;
            }
        }
    }
    /// <summary>
    /// Физика управления
    /// </summary>
    public void Steering()
    {
        currentSteeringWheelAngle = (maxSteeringAngle * steeringWheelCoefficient) * Input.GetAxis("Horizontal"); //подсчет угла руля
        currentSteeringAngle = currentSteeringWheelAngle / steeringWheelCoefficient; //подсчет угла выворота колес
        //currentSteeringAngle = maxSteeringAngle * Input.GetAxis("Horizontal");
        frontLeftWheel.steerAngle = currentSteeringAngle;
        frontRightWheel.steerAngle = currentSteeringAngle;

        
    }
    /// <summary>
    /// Совмещает координаты коллайдера колес и 3д модели
    /// </summary>
    public void ModelPosition()
    {
        //переднее левое
        Vector3 fLWPosition;
        Quaternion fLWrotation;
        frontLeftWheel.GetComponent<WheelCollider>().GetWorldPose(out fLWPosition, out fLWrotation);
        frontLeftWheelModel.transform.rotation = fLWrotation;
        frontLeftWheelModel.transform.position = fLWPosition;
        //переднее правое
        Vector3 fRWPosition;
        Quaternion fRWrotation;
        frontRightWheel.GetComponent<WheelCollider>().GetWorldPose(out fRWPosition, out fRWrotation);
        frontRightWheelModel.transform.rotation = fRWrotation;
        frontRightWheelModel.transform.position = fRWPosition;
        //заднее левое
        Vector3 rLWPosition;
        Quaternion rLWrotation;
        rearLeftWheel.GetComponent<WheelCollider>().GetWorldPose(out rLWPosition, out rLWrotation);
        rearLeftWheelModel.transform.rotation = rLWrotation;
        rearLeftWheelModel.transform.position = rLWPosition;
        //заднее правое
        Vector3 rRWPosition;
        Quaternion rRWrotation;
        rearRightWheel.GetComponent<WheelCollider>().GetWorldPose(out rRWPosition, out rRWrotation);
        rearRightWheelModel.transform.rotation = rRWrotation;
        rearRightWheelModel.transform.position = rRWPosition;
    }
}

//передачи
[System.Serializable]
public class Gears
{
    //ратио
    public float ratio = 0;
    //для АКПП - необходимое кол-во оборотов для переключения
    public int revsToGearUp = 0;
}