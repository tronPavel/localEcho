import { useMap } from 'react-leaflet';
import { toast } from 'sonner';
import styles from './PositionControl.module.css';

export const PositionControl = () => {
    const map = useMap();

    const handleLocate = () => {
        const promise = new Promise((resolve, reject) => {
            map.locate({ setView: true, maxZoom: 16 });
            map.once('locationfound', (e) => resolve(e));
            map.once('locationerror', (err) => reject(err));
        });

        toast.promise(promise, {
            loading: 'Определяем координаты...',
            success: 'Вы здесь!',
            error: 'Не удалось получить доступ к GPS',
        });
    };

    return (
        <div className={styles.container}>
            <button className={styles.btn} onClick={handleLocate} title="Найти меня">
                🎯
            </button>
        </div>
    );
};