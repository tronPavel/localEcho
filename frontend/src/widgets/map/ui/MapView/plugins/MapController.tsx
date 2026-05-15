import { useAuthStore } from "@/features/auth/model/authStore.ts";
import { useMap } from "react-leaflet";
import { useEffect } from "react";
import L from "leaflet";
import { useCityStore } from "@/features/city-selector/model/cityStore.ts";

export const MapController = () => {
    const map = useMap();
    const user = useAuthStore(s => s.user);
    const mapFocus = useCityStore(s => s.mapFocus);

    useEffect(() => {
        if (mapFocus) {
            map.flyTo(mapFocus, 12, { duration: 1.5 });
            return;
        }

        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    const { latitude, longitude } = pos.coords;
                    map.flyTo([latitude, longitude], 15);
                    // Удаляем старые маркеры точности если были
                    L.circle([latitude, longitude], { radius: 10, color: 'blue' }).addTo(map);
                },
                () => {
                    if (user?.latitude && user?.longitude) {
                        map.setView([user.latitude, user.longitude], 14);
                    }
                }
            );
        }
    }, [map, mapFocus]);

    return null;
};