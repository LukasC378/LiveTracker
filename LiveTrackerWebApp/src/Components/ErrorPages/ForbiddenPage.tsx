import {Link} from "react-router-dom";

const ForbiddenPageComponent = () => {
    return (
        <div className={"p-4"}>
            <h1>You do not have permission</h1>
            <Link to={"/"}>Home</Link>
        </div>
    );
}

export default ForbiddenPageComponent;