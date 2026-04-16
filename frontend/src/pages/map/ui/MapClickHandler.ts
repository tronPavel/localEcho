import {useAuthStore} from "@/features/auth/model/authStore.ts";
import {useMapEvents} from "react-leaflet";
import {useCreateMarkerStore} from "@/features/create-marker/model/createMarkerStore.ts";

export const MapClickHandler = () => {
    const { setPendingPosition, openModal } = useCreateMarkerStore();
    const { isAuthenticated } = useAuthStore();
    useMapEvents({
        click(e) {
            if (isAuthenticated) {
                setPendingPosition({ lat: e.latlng.lat, lng: e.latlng.lng });
                openModal();
            }
        },
    });
    return null;
};