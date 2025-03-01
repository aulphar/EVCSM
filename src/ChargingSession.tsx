import { useState } from "react";
import './ChargingSession.css';  // Import the CSS file

export default function ChargingSession() {
    const [charging, setCharging] = useState<boolean>(false);
    const [energyConsumed, setEnergyConsumed] = useState<number>(0);
    const [status, setStatus] = useState<string>("Idle");

    const startCharging = () => {
        setCharging(true);
        setStatus("Charging");
        setEnergyConsumed(0);

        // Simulating energy consumption increase
        const interval = setInterval(() => {
            setEnergyConsumed((prev) => prev + 0.5);
        }, 1000);

        setTimeout(() => clearInterval(interval), 20000); // Stops simulation after 20s
    };

    const stopCharging = () => {
        setCharging(false);
        setStatus("Stopped");
    };

    return (
        <div>
            <h1>Electric Vehicle Charging Session</h1>
            <p>Status: {status}</p>
            <p>Energy Consumed: {energyConsumed.toFixed(2)} kWh</p>
            <button onClick={startCharging} disabled={charging}>Start Charging</button>
            <button onClick={stopCharging} disabled={!charging}>Stop Charging</button>
        </div>
    );
}
