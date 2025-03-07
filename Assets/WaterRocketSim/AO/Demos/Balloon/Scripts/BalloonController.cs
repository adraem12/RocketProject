using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class BalloonController : MonoBehaviour
    {
        [Range(0.5f, 2f)]
        public float balloonDiameter = 1;
        public Transform balloonGraphic, balloonAeroObject, stringAttachment;
        public ConfigurableJoint configurableJoint;
        public float stringLength;
        SoftJointLimit sjl;
        public WindArrow windArrow;
        public WeightArrow weightArrow;
        public DragArrow dragArrow;
        public BuoyancyArrow buoyancyArrow;
        public Slider inflateSlider, windSpeedSlider, stringLengthSlider;
        public DefaultFluidZone fluidZone;

        // Start is called before the first frame update
        void Start()
        {

            fluidZone.windVelocity.speed = 1;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            balloonGraphic.localScale = balloonDiameter * Vector3.one;
            balloonAeroObject.localScale = balloonDiameter * Vector3.one;
            stringAttachment.localPosition = new Vector3(0, -balloonDiameter / 2, 0);
            windArrow.Offset = -balloonDiameter / 2;
            weightArrow.Offset = balloonDiameter / 2;
            dragArrow.Offset = balloonDiameter / 2;
            buoyancyArrow.Offset = balloonDiameter / 2;
            configurableJoint.anchor = stringAttachment.localPosition;
            sjl.limit = stringLength;
            configurableJoint.linearLimit = sjl;
        }
        public void updateBalloon()
        {
            balloonDiameter = inflateSlider.value;
        }
        public void updateWind()
        {
            fluidZone.windVelocity.speed = windSpeedSlider.value;
        }
        public void updateStringLength()
        {
            stringLength = stringLengthSlider.value;
        }
    }
}