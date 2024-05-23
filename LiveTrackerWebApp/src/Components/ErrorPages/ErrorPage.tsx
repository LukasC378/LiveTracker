import {Link} from "react-router-dom";

const ErrorPageComponent = () => {
    return (
        <div className={"p-4"}>
            <h1>Something went wrong</h1>
            <Link to={"/"}>Home</Link>
        </div>
    );
}

export default ErrorPageComponent