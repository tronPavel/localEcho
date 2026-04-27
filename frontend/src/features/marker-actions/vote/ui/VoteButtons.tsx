import { useVote } from '../model/useVote';
import { Button } from "@/shared/ui/Button/Button";
import { Icon } from "@/shared/ui/Icon/Icon";
import styles from './VoteButtons.module.css';

interface VoteButtonsProps {
    markerId: string;
    currentVote: number;
    rating: number;
}

export const VoteButtons = ({ markerId, currentVote, rating }: VoteButtonsProps) => {
    const { mutate, isPending } = useVote(markerId);

    return (
        <div className={styles.buttons}>
            <Button
                variant="outline"
                size="small"
                onClick={() => mutate(true)}
                className={currentVote === 1 ? styles.active : ''}
                disabled={isPending}
            >
                <Icon type="up" /> {rating > 0 ? `+${rating}` : rating}
            </Button>
            <Button
                variant="outline"
                size="small"
                onClick={() => mutate(false)}
                className={currentVote === -1 ? styles.active : ''}
                disabled={isPending}
            >
                <Icon type="down" />
            </Button>
        </div>
    );
};