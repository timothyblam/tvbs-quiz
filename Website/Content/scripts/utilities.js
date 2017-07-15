// The scope must be applied manually because web socket requests are not part of the angular lifecycle.
function applyScopeManually(scope, fn) {
	if (!scope.$root.$$phase) {
		scope.$apply(fn);
	} else {
		fn();
	}
}