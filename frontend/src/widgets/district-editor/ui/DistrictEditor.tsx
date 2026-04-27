import { MapContainer, TileLayer, useMap, Polygon as LeafletPolygon } from 'react-leaflet';
import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import L from 'leaflet';
import '@geoman-io/leaflet-geoman-free';
import styles from './DistrictEditor.module.css';
import {districtApi} from "@/entities/district/model/districtApi.ts";

interface DistrictEditorProps {
    initialGeometry: { lat: number, lng: number }[];
    onSave: (coords: { lat: number, lng: number }[]) => void;
    excludeId?: string;
}

/**
 * Слой отображения существующих районов (только для чтения)
 */
const BackgroundDistricts = ({ excludeId }: { excludeId?: string }) => {
    const { data: districts = [] } = useQuery({
        queryKey: ['districts-map'],
        queryFn: districtApi.getForMap,
        staleTime: 1000 * 60 * 5
    });

    return (
        <>
            {districts
                .filter(d => d.id !== excludeId)
                .map(d => (
                    <LeafletPolygon
                        key={d.id}
                        positions={d.geometry.map(c => [c.lat, c.lng])}
                        pathOptions={{
                            color: '#464750',
                            fillColor: '#cbd5e1',
                            fillOpacity: 0.9,
                            weight: 1,
                            dashArray: '5, 5',
                            interactive: false
                        }}
                    />
                ))}
        </>
    );
};

const GeomanController = ({ geometry, onChange }: {
    geometry: any[],
    onChange: (coords: any[]) => void
}) => {
    const map = useMap();

    useEffect(() => {
        if (!map) return;
        map.pm.setLang('ru');

        if (geometry.length > 0) {
            const polygon = L.polygon(geometry.map(c => [c.lat, c.lng])).addTo(map);
            polygon.pm.enable({
                allowSelfIntersection: false,
                snappable: true,
                snapDistance: 5,     
            });

            const sync = () => {
                const layers = polygon.getLatLngs() as any;
                onChange(layers[0].map((c: any) => ({ lat: c.lat, lng: c.lng })));
            };

            polygon.on('pm:edit', sync);
            polygon.on('pm:markerdragend', sync);
            map.fitBounds(polygon.getBounds(), { padding: [40, 40] });
            return () => { map.removeLayer(polygon); };
        } else {
            map.pm.enableDraw('Polygon', {
                snappable: true,
                snapDistance: 5,
                allowSelfIntersection: false,
                templineStyle: { color: '#064e3b' },
                hintlineStyle: { color: '#064e3b', dashArray: '5,5' }
            });

            map.on('pm:create', (e: any) => {
                const coords = e.layer.getLatLngs()[0].map((c: any) => ({ lat: c.lat, lng: c.lng }));
                onChange(coords);
            });

            return () => { map.off('pm:create'); map.pm.disableDraw(); };
        }
    }, [map, geometry.length]);

    return null;
};

export const DistrictEditor = ({ initialGeometry, onSave, excludeId }: DistrictEditorProps) => {
    const [currentCoords, setCurrentCoords] = useState(initialGeometry);

    return (
        <div className={styles.wrapper}>
            <div className={styles.mapBox}>
                <MapContainer center={[53.9, 27.56]} zoom={11} className={styles.leaflet}>
                    <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />


                    <BackgroundDistricts excludeId={excludeId} />


                    <GeomanController geometry={initialGeometry} onChange={setCurrentCoords} />
                </MapContainer>
            </div>
            <div className={styles.footer}>
                <p className={styles.hint}>
                    Серым пунктиром отмечены границы других районов. Не допускайте наслоений.
                </p>
                <button
                    className={styles.saveBtn}
                    onClick={() => onSave(currentCoords)}
                >
                    💾 Сохранить границы
                </button>
            </div>
        </div>
    );
};