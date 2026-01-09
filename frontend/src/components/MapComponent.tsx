import { MapContainer, TileLayer, Marker, Popup, useMapEvents } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';
import {useUIStore} from "../store";
import {useMarkers} from "../features/markers/api/Markers.query.ts";
import {CreateMarkerForm} from "../features/markers/components/MarkerForm.tsx";

const DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41],
});
L.Marker.prototype.options.icon = DefaultIcon;

const MapClickHandler = () => {
    const { setPendingMarker, openCreateMarkerModal } = useUIStore();

    useMapEvents({
        click(e) {
            const { lat, lng } = e.latlng;
            setPendingMarker(lat, lng);
            openCreateMarkerModal();
        },
    });

    return null;
};

export const MapComponent: React.FC = () => {
    const { data: markers = [] } = useMarkers();
    const { isCreateMarkerModalOpen } = useUIStore();

    return (
        <>
            <MapContainer center={[55.7558, 37.6173]} zoom={13} style={{ height: '100vh', width: '100%' }}>
                <TileLayer
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    attribution='&copy; OpenStreetMap contributors'
                />

                <MapClickHandler />

                {markers.map((marker) => (
                    <Marker key={marker.id} position={[marker.latitude, marker.longitude]}>
                        <Popup>
                            <strong>{marker.title}</strong>
                            {marker.description && <p>{marker.description}</p>}
                            <small>Категория: {marker.category}</small>
                        </Popup>
                    </Marker>
                ))}
            </MapContainer>

            {/* Модалка с формой — рендерится только когда открыта */}
            {isCreateMarkerModalOpen && <CreateMarkerForm />}
        </>
    );
};

export default MapComponent;