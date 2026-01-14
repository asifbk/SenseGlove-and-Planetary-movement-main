# Haptic Vibration Fix - GrabVibration.cs

## Problem Identified

The haptic vibration was providing the same intensity for 0 m/s² and 5 m/s² acceleration due to incorrect amplitude calculation logic.

### Issues Found:

1. **Incorrect amplitude calculation** - Used `force / maxForce` which depended on a manually set `maxForce` value that was not calibrated per planet
2. **Unused sinusoidal modulation** - Calculated modulated amplitudes but never sent them to the glove
3. **Configuration mismatch** - `maxForce` was set to 10, but for Earth (mass=350, aMax=5), the actual max force would be 1750

## Solution Implemented

### Changed Amplitude Calculation

**Old Method:**
```csharp
float force = mass * accelClamped;
float aRaw = force / maxForce;  // Depended on incorrect maxForce value
float aClamped = Mathf.Clamp01(aRaw);
```

**New Method:**
```csharp
// Amplitude scales linearly from 0 to 1 as acceleration goes from aMin to aMax
float accelNorm = (accelClamped - aMin) / (aMax - aMin);
float aClamped = Mathf.Clamp01(accelNorm);
```

### Benefits of New Approach:

1. **Direct mapping** - Amplitude is directly proportional to acceleration (normalized between aMin and aMax)
2. **No configuration needed** - Removed the `maxForce` field that required manual calibration
3. **Simpler logic** - Removed unused sinusoidal modulation code
4. **Consistent behavior** - Works correctly for all planets regardless of mass

## How It Works Now

### Acceleration to Vibration Mapping:

| Acceleration (m/s²) | Normalized Value | Vibration Amplitude |
|---------------------|------------------|---------------------|
| 0 - 0.1             | 0%               | **No vibration** (stops) |
| 0.1                 | 0%               | Minimum vibration |
| 2.55 (midpoint)     | 50%              | Medium vibration |
| 5.0                 | 100%             | **Maximum vibration** |
| > 5.0               | 100% (clamped)   | **Maximum vibration** (locked) |

### Per-Channel Amplitudes:

Each channel (thumb, index, wrist) has its own amplitude multiplier:
- **Thumb**: `amplitude * thumbAmplitude` (e.g., 0.35)
- **Index**: `amplitude * indexAmplitude` (e.g., 0.35)
- **Wrist**: `amplitude * wristAmplitude` (e.g., 0.35)

### Frequency Modulation:

Frequency still varies with acceleration:
- **Low acceleration** (0.1 m/s²): 100 Hz
- **Medium acceleration** (2.55 m/s²): 140 Hz
- **High acceleration** (5.0+ m/s²): 180 Hz

## Testing Recommendations

1. **At rest** (0 m/s²): No vibration should occur
2. **Slow movement** (0.1-1 m/s²): Light vibration, low frequency
3. **Moderate movement** (1-3 m/s²): Medium vibration, medium frequency
4. **Fast movement** (3-5 m/s²): Strong vibration, high frequency
5. **Very fast movement** (>5 m/s²): Maximum vibration (locked at 100%), maximum frequency

## Changes Summary

### Removed:
- ❌ `maxForce` field (no longer needed)
- ❌ `dynamicTime` variable (unused sinusoidal modulation)
- ❌ Sinusoidal modulation calculation
- ❌ Force-based amplitude calculation

### Modified:
- ✅ Direct acceleration-to-amplitude mapping
- ✅ Simplified amplitude calculation
- ✅ Cleaner code without unused logic

### Preserved:
- ✅ Acceleration threshold checking (aMin = 0.1 m/s²)
- ✅ Acceleration clamping (aMax = 5.0 m/s²)
- ✅ Frequency modulation based on acceleration
- ✅ Per-channel amplitude control
- ✅ All logging and UI display functionality
- ✅ SenseGlove haptic feedback integration
