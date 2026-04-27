import {useAuthStore} from "@/features/auth/model/authStore.ts";
import {useMap} from "react-leaflet";
import {useEffect} from "react";
import L from "leaflet";

export const MapController = () => {
    const map = useMap();
    const user = useAuthStore(s => s.user);

    useEffect(() => {
        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    const { latitude, longitude } = pos.coords;
                    map.flyTo([latitude, longitude], 15);
                    L.circle([latitude, longitude], { radius: 10 }).addTo(map);
                },
                () => {
                    if (user?.latitude && user?.longitude) {
                        map.setView([user.latitude, user.longitude], 14);
                    } else if (user?.districtId) {
                        // Позже добавим flyTo на центр района
                    }
                }
            );
        }
    }, [map, user]);

    return null;
};