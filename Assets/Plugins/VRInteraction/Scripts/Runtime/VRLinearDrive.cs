﻿using System;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR.InteractionSystem;

namespace GameLabGraz.VRInteraction
{
	public class VRLinearDrive : LinearDrive
	{
		public bool _showDebugMessages = false;
		
		[Header("VR Interaction Plugin")] public bool useSteps = true;
		[Range(0f, 10f)] public float stepSize = 1f;
		public bool useAsInteger = false;

		public float initialValue = 5f;
		public float minimum = 0f;
		public float maximum = 10f;

		public ValueChangeEventFloat onValueChanged;
		public ValueChangeEventInt onValueChangedInt;

        public ValueChangeEventFloat onRelease;
        public ValueChangeEventInt onReleaseInt;

		protected float _currentValue;
		protected float _valueRange;

		protected override void Start()
		{
			if ( linearMapping == null )
				linearMapping = GetComponent<LinearMapping>();
			if ( linearMapping == null )
				linearMapping = gameObject.AddComponent<LinearMapping>();

			Debug.Assert(linearMapping != null);
			initialMappingOffset = linearMapping.value;

			if ( repositionGameObject )
				UpdateLinearMapping( transform );
			
			_valueRange = maximum - minimum;
			_currentValue = Mathf.Clamp(initialValue, minimum, maximum);
			linearMapping.value = (_currentValue - minimum) / _valueRange;
			// Debug.Log("linear mapping val: " + linearMapping + " - " + linearMapping.value);
			transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
			
			onValueChanged.Invoke(_currentValue);
			if(useAsInteger)
				onValueChangedInt.Invoke(Mathf.RoundToInt(_currentValue));
		}
		
		public virtual void SetMinMax(float min, float max)
		{
			minimum = min;
			maximum = max;
			AdaptMinMax();
		}

		protected virtual void AdaptMinMax()
		{
			_valueRange = maximum - minimum;
			_currentValue = Mathf.Clamp(_currentValue, minimum, maximum);
			linearMapping.value = (_currentValue - minimum) / _valueRange;
			// Debug.Log("linear mapping val: " + linearMapping + " - " + linearMapping.value);
			transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);

			onValueChanged.Invoke(_currentValue);
			if(useAsInteger)
				onValueChangedInt.Invoke(Mathf.RoundToInt(_currentValue));
		}
		
		protected override void HandAttachedUpdate(Hand hand)
		{
			UpdateLinearMapping(hand.transform);

			if (hand.IsGrabEnding(gameObject))
			{
				hand.DetachObject(gameObject);

				onRelease?.Invoke(_currentValue);
				if (useAsInteger)
					onReleaseInt?.Invoke(Mathf.RoundToInt(_currentValue));
			}
		}
		
		protected new void UpdateLinearMapping(Transform updateTransform)
		{
			prevMapping = linearMapping.value;
			linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

			_currentValue = Mathf.Clamp(linearMapping.value * _valueRange + minimum, minimum, maximum);
			if (useAsInteger)
				_currentValue = Mathf.RoundToInt(_currentValue);

			if (useSteps)
			{
				var tmp = _currentValue % stepSize;
				if (tmp > stepSize / 2)
					_currentValue = _currentValue - tmp + stepSize;
				else
					_currentValue -= tmp;
				
				if (useAsInteger)
					_currentValue = Mathf.RoundToInt(_currentValue);

				linearMapping.value = (_currentValue - minimum) / _valueRange;
			}

			onValueChanged.Invoke(_currentValue);
			if(useAsInteger)
				onValueChangedInt.Invoke(Mathf.RoundToInt(_currentValue));
			
			mappingChangeSamples[sampleCount % mappingChangeSamples.Length] =
				(1.0f / Time.deltaTime) * (linearMapping.value - prevMapping);
			sampleCount++;

			if (repositionGameObject && !float.IsNaN(linearMapping.value))
			{
				var startPos = startPosition.position;
				var endPos = endPosition.position;
				transform.position = Vector3.Lerp(startPos, endPos, linearMapping.value);
			}
		}

		public void ForceToValue(float newValue)
		{
			if(_showDebugMessages)
				Debug.Log("VRInteraction:VRLinearDrive: Force to Value: " + newValue);
			
			prevMapping = linearMapping.value;
			_currentValue = Mathf.Clamp(newValue, minimum, maximum);
			if (useAsInteger)
				_currentValue = Mathf.RoundToInt(_currentValue);

			if (useSteps)
			{
				var tmp = _currentValue % stepSize;
				if (tmp > stepSize / 2)
					_currentValue = _currentValue - tmp + stepSize;
				else
					_currentValue -= tmp;
				
				if (useAsInteger)
					_currentValue = Mathf.RoundToInt(_currentValue);
			}
			linearMapping.value = (_currentValue - minimum) / _valueRange;

			onValueChanged.Invoke(_currentValue);
			if(useAsInteger)
				onValueChangedInt.Invoke(Mathf.RoundToInt(_currentValue));
			
			mappingChangeSamples[sampleCount % mappingChangeSamples.Length] =
				(1.0f / Time.deltaTime) * (linearMapping.value - prevMapping);
			sampleCount++;

			if (repositionGameObject && !float.IsNaN(linearMapping.value))
			{
				var startPos = startPosition.position;
				var endPos = endPosition.position;
				transform.position = Vector3.Lerp(startPos, endPos, linearMapping.value);
			}
		}
	}
}