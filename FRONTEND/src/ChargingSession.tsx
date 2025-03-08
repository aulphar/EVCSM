import { useState, useEffect } from "react";
import mqtt, { MqttClient } from "mqtt";
import './ChargingSession.css';

export default function ChargingSession() {
    const [charging, setCharging] = useState<boolean>(false);
    const [energyConsumed, setEnergyConsumed] = useState<string>("0");
    const [status, setStatus] = useState<string>("Idle");
    const [client, setClient] = useState<MqttClient | null>(null);

    useEffect(() => {
        const mqttClient = mqtt.connect('ws://ec2-51-20-48-234.eu-north-1.compute.amazonaws.com:9001', {
            protocol: "ws",
            protocolVersion: 5,
            clean: true,
            will: {
                topic: "charging/updates",
                payload: JSON.stringify({status: "Stopped", reason: "Unexpected Disconnect", endTime: Date.now()}),
                qos: 1,
                retain: false
            }
        });

        mqttClient.on("connect", () => {
            console.log("Connected to MQTT broker.");
            mqttClient.subscribe("charging/updates", () => {
                console.log("Subscribed to Charging Updates");
            });
            mqttClient.subscribe("session/updates", () => {
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
            }
             if (topic === "session/updates") {
                const payload = JSON.parse(message.toString());
                console.log("Received energy consumption update:", payload);
                if (typeof payload.energyConsumed === "number") {
                    setEnergyConsumed(payload.energyConsumed);
                    console.log(payload);
                }
             }
        });

        setClient(() => mqttClient);

        return () => {
            console.log("Disconnecting MQTT client...");
            mqttClient.end();
        };
    }, []);

    const publishMessage = (payload: object) => {
        if (client) {
            client.publish("charging/updates", JSON.stringify(payload));
        }
    };

    const startCharging = () => {
        setCharging(true);
        publishMessage({
            status: "Charging",
            startTime: new Date().toISOString(),
        });
    };

    const stopCharging = () => {
        setCharging(false);
        publishMessage({
            status: "Stopped",
            endTime: new Date().toISOString(),
        });
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
