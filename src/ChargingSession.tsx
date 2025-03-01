import { useState } from "react";
import mqtt from "mqtt";
import './ChargingSession.css';

export default function ChargingSession() {
    const [charging, setCharging] = useState<boolean>(false);
    const [energyConsumed, setEnergyConsumed] = useState<number>(0);
    const [status, setStatus] = useState<string>("Idle");

    const client = mqtt.connect('ws://127.0.0.1:9001');
    client.on("connect", () => {
        client.subscribe("charging/updates",() => {console.log("Suscribed to Charging Updates")});
        console.log("Connected to MQTT broker.");
    });
    client.on("message", (topic, message) => {
        if (topic === "charging/updates") {
            const payload = JSON.parse(message.toString());
            console.log("Received update:", payload);
            if (payload.status)
            {
                setStatus(payload.status);
            }
        }

    });

    const startCharging = () => {

        const topic = 'charging/updates';
        const payload = JSON.stringify({ status: "Charging", timestamp: new Date().toISOString() });
        client.publish(topic, payload);
        setCharging(true);
        setEnergyConsumed(0);
    };

    const stopCharging = () => {
        setCharging(false);
        const topic = 'charging/updates';
        const payload = JSON.stringify({ status: "Stopped", timestamp: new Date().toISOString() });
        client.publish(topic, payload);
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
