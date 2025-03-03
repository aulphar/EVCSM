import { useState, useEffect } from "react";
import mqtt, { MqttClient } from "mqtt";
import './ChargingSession.css';

export default function ChargingSession() {
    const [charging, setCharging] = useState<boolean>(false);
    const [energyConsumed, setEnergyConsumed] = useState<number>(0);
    const [status, setStatus] = useState<string>("Idle");
    const [client, setClient] = useState<MqttClient | null>(null);

    useEffect(() => {
        const mqttClient = mqtt.connect("ws://127.0.0.1:9001", {
            protocol: "ws",
            protocolVersion: 5,
            clean: true,
        });

        mqttClient.on("connect", () => {
            mqttClient.subscribe("charging/updates", () => {
                console.log("Subscribed to Charging Updates");
            });
            console.log("Connected to MQTT broker.");
        });

        mqttClient.on("message", (topic, message) => {
            if (topic === "charging/updates") {
                const payload = JSON.parse(message.toString());
                console.log("Received update:", payload);

                if (payload.status) {
                    setStatus(() => payload.status);
                }
            }
        });

        setClient(mqttClient);

        return () => {
            console.log("Disconnecting MQTT client...");
            mqttClient.end();
        };
    }, []);

    useEffect(() => {
        let interval: NodeJS.Timeout | null = null;

        if (charging) {
            interval = setInterval(() => {
                setEnergyConsumed((prev) => prev + 0.1);
            }, 1000);
        } else {
            if (interval) clearInterval(interval);
        }

        return () => {
            if (interval) clearInterval(interval);
        };
    }, [charging]);

    const publishMessage = (status: string) => {
        if (client) {
            const topic = "charging/updates";
            const payload = JSON.stringify({ status, timestamp: new Date().toISOString() });
            client.publish(topic, payload);
        }
    };

    const startCharging = () => {
        setCharging(true);
        setEnergyConsumed(0);
        publishMessage("Charging");
    };

    const stopCharging = () => {
        setCharging(false);
        publishMessage("Stopped");
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
