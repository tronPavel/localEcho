import { useVote } from '../model/useVote';
import { Button } from "@/shared/ui/Button/Button";
import { Icon } from "@/shared/ui/Icon/Icon";
import styles from './VoteButtons.module.css';
import {useAuthStore} from "@/features/auth/model/authStore.ts";
import {useLocation, useNavigate} from "react-router-dom";
import {toast} from "sonner";

interface VoteButtonsProps {
    markerId: string;
    currentVote: number;
    rating: number;
}

export const VoteButtons = ({ markerId, currentVote, rating }: VoteButtonsProps) => {
    const { isAuthenticated } = useAuthStore();
    const navigate = useNavigate();
    const location = useLocation();
    const { mutate, isPending } = useVote(markerId);

    const handleVote = (isUp: boolean) => {
        if (!isAuthenticated) {
            toast.info("Войдите, чтобы проголосовать");
            navigate('/login', { state: { from: location } });
            return;
        }
        mutate(isUp);
    };


    return (
        <div className={styles.buttons}>
            <Button
                variant="outline"
                size="small"
                onClick={() => handleVote(true)}
                className={currentVote === 1 ? styles.active : ''}
                disabled={isPending}
            >
                <Icon type="up" /> {rating > 0 ? `+${rating}` : rating}
            </Button>
            <Button
                variant="outline"
                size="small"
                onClick={() => handleVote(false)}
                className={currentVote === -1 ? styles.active : ''}
                disabled={isPending}
            >
                <Icon type="down" />
            </Button>
        </div>
    );
};