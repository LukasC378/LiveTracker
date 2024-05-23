import {ProgressBar} from "react-bootstrap";
import {Utils} from "../../Utils/Utils";

const PasswordProgressBar = (props: { password: string }) => {
    const percentage = Utils.resultPasswordProgressBarPercentage(props.password);
    const animated = percentage != 100;
    const variant = Utils.resultPasswordProgressBarVariant(props.password);

    return(
        <div>
            <ProgressBar style={{height: 7, marginTop: '7px'}} animated={animated} variant={variant} now={percentage} />
            Password strength
        </div>
    );
}

export default PasswordProgressBar;