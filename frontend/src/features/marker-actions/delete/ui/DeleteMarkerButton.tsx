import { useDeleteMarker } from '../model/useDeleteMarker';
import { Button } from "@/shared/ui/Button/Button";
import styles from './DeleteMarkerButton.module.css';

export const DeleteMarkerButton = ({ markerId }: { markerId: string }) => {
    const { mutate, isPending } = useDeleteMarker();

    const handleDelete = () => {
        if (window.confirm("Вы уверены, что хотите безвозвратно удалить эту метку?")) {
            mutate(markerId);
        }
    };

    return (
        <Button
            variant="outline"
            size="small"
            className={styles.deleteBtn}
            onClick={handleDelete}
            disabled={isPending}
        >
            {isPending ? "..." : "🗑 Удалить"}
        </Button>
    );
};