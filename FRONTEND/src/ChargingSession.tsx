import { useState, useEffect } from "react";
import mqtt, { MqttClient } from "mqtt";
import './ChargingSession.css';

export default function ChargingSession() {
    const [charging, setCharging] = useState<boolean>(false);
    const [energyConsumed, setEnergyConsumed] = useState<string>("0");
    const [status, setStatus] = useState<string>("Idle");
    const [client, setClient] = useState<MqttClient | null>(null);
    const [clientId, setClientId] = useState<string>("");

    //useEffect so MqttClient doesn't reset for every re-render

    useEffect(() => {
        const mqttClient = mqtt.connect("ws://127.0.0.1:9001", {
            clientId: clientId,
            protocol: "ws",
            protocolVersion: 5,
            clean: false,
            reconnectPeriod: 1000,
        });

        mqttClient.on("connect", () => {
            console.log("Connected to MQTT broker.");
            mqttClient.subscribe("charging/updates", () => {
                console.log("Subscribed to Charging Updates");
            });
            mqttClient.subscribe("energyconsumed", () => {
                console.log("Subscribed to Energy Consumed Updates");
            });
        });

        mqttClient.on("message", (topic, message) => {
            if (topic === "charging/updates") {
                const payload = JSON.parse(message.toString());
                console.log("Received update:", payload);

                if (payload.status) {
                    setStatus(() => payload.status);
                }
            }else if (topic === "energyconsumed") {
                const payload = JSON.parse(message.toString());
                console.log("Received energy consumption update:", payload);
                if (typeof payload.energyConsumed === "number") {
                    setEnergyConsumed(payload.energyConsumed);
                    console.log(payload.energyConsumed);
                }}
        });

        setClient(() => mqttClient);

        return () => {
            console.log("Disconnecting MQTT client...");
            mqttClient.end();
        };
    }, []);

    const publishMessage = (status: string) => {
        if (client) {
            const topic = "charging/updates";
            const payload = JSON.stringify({ status, startTime: new Date().toISOString() });
            client.publish(topic, payload);
        }
    };

    const startCharging = () => {
        setCharging(true);
        setEnergyConsumed(energyConsumed);
        publishMessage("Charging");
    };

    const stopCharging = () => {
        setCharging(false);
        setEnergyConsumed(energyConsumed);
        publishMessage("Stopped");
    };

    return (
        <div>
            <h1>Electric Vehicle Charging Session</h1>
            <p>Status: {status}</p>
            <p>Energy Consumed: {energyConsumed} kWh</p>
            <button onClick={startCharging} disabled={charging}>Start Charging</button>
            <button onClick={stopCharging} disabled={!charging}>Stop Charging</button>
        </div>
    );
}
